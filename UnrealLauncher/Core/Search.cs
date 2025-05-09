using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace UnrealLauncher.Core;

public enum ExecCode
{
    Success = 0,
    FileNotFound = 1,
    NotWindows = 2,
    RegeditNotFound = 3,
    PathIsNull = 4,
    FileOccupying = 5,
    UEisRunning = 6
}

public static partial class Search
{
    private static readonly string UnrealAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UnrealEngine");
    private static readonly string UnrealEditorSettingsPath = Path.Combine("Saved", "Config", "WindowsEditor", "EditorSettings.ini");
    public const string UnrealNoPicFoundImagePath = "avares://UnrealLauncher/Assets/no_pic.png";

    private static IEnumerable<string> GetEngineVersionList()
    {
        return Directory.GetDirectories(UnrealAppDataPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(dir => SearchEnginePath().IsMatch(Path.GetFileName(dir)));
    }

    private static string GetAutoScreenshotPath(string uProjectPath)
    {
        var candidate1 = Path.ChangeExtension(uProjectPath, ".png");
        var candidate2 = Path.Combine(
            Path.GetDirectoryName(uProjectPath) ?? "",
            "Saved", "AutoScreenshot.png"
        );

        if (FileOps.IsFileExists(candidate1)) return candidate1;
        if (FileOps.IsFileExists(candidate2)) return candidate2;
        return UnrealNoPicFoundImagePath;
    }

    public static (string, string) GetUnrealProjectContextMenuFromRegedit(out ExecCode execCode)
    {
        if (!OperatingSystem.IsWindows())
        {
            execCode = ExecCode.NotWindows;
            return (string.Empty, string.Empty);
        }

        const string keyPath1 = @"Unreal.ProjectFile\shell\rungenproj\command";
        const string keyPath2 = @"Unreal.ProjectFile\shell\switchversion\command";

        using var commandKey1 = Registry.ClassesRoot.OpenSubKey(keyPath1);
        using var commandKey2 = Registry.ClassesRoot.OpenSubKey(keyPath2);

        if (commandKey1 == null || commandKey2 == null)
        {
            execCode = ExecCode.RegeditNotFound;
            return (string.Empty, string.Empty);
        }

        var command1 = commandKey1.GetValue(null) as string ?? string.Empty;
        var command2 = commandKey2.GetValue(null) as string ?? string.Empty;

        execCode = ExecCode.Success;
        return (command1, command2);
    }

    public static void SortByDate(List<UnrealProject> list)
    {
        list.Sort((a, b) => string.Compare(b.LastAccessDate, a.LastAccessDate, StringComparison.Ordinal));
    }

    public static void GetAllRecentlyOpenedProjects(List<UnrealProject> unrealProjectsList)
    {
        List<string> editorSettingsPathList = [];

        // Get all editor settings files from the engine version directories
        editorSettingsPathList.AddRange(GetEngineVersionList().Select(dir => Path.Combine(dir, UnrealEditorSettingsPath)).Where(FileOps.IsFileExists));

        // Get all lines contains "RecentlyOpenedProjectFiles" from the editor settings files
        foreach (var file in editorSettingsPathList)
        {
            foreach (var line in FileOps.ReadLines(file))
            {
                var projectPathMatch = SearchProjectPath().Match(line);

                if (!projectPathMatch.Success) continue;
                // Check if the project path is valid
                if (!FileOps.IsFileExists(projectPathMatch.Groups[1].Value)) continue;

                unrealProjectsList.Add(
                    new UnrealProject(
                        GetAutoScreenshotPath(projectPathMatch.Groups[1].Value),
                        projectPathMatch.Groups[1].Value
                    ));
            }
        }
    }


    [GeneratedRegex("""
                    ^\d+\.\d+$
                    """)]
    private static partial Regex SearchEnginePath();

    [GeneratedRegex("""
                    ProjectName="([^"]+)"
                    """)]
    private static partial Regex SearchProjectPath();
}