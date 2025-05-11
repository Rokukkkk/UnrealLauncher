using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Win32;

namespace UnrealLauncher.Core;

public static partial class Search
{
    public class UnrealProject(string thumbnailPath, string uProjectPath)
    {
        public Bitmap Thumbnail { get; } = thumbnailPath == UnrealNoPicFoundImagePath ? new Bitmap(AssetLoader.Open(new Uri(thumbnailPath))) : new Bitmap(thumbnailPath);
        public string UProjectPath { get; } = uProjectPath;
        public string UProjectName { get; } = FileOps.GetFileNameWithoutExtension(uProjectPath);
        public string LastAccessDate { get; } = FileOps.GetLastAccessTime(uProjectPath);
    }

    private static readonly string UnrealAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UnrealEngine");
    private static readonly string UnrealEditorSettingsPath = Path.Combine("Saved", "Config", "WindowsEditor", "EditorSettings.ini");
    private const string UnrealNoPicFoundImagePath = "avares://UnrealLauncher/Assets/no_pic.png";

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

    private static void SortByDate(List<UnrealProject> list)
    {
        list.Sort((a, b) => string.Compare(b.LastAccessDate, a.LastAccessDate, StringComparison.Ordinal));
    }

    private static void GetAllRecentlyOpenedProjects(List<UnrealProject> unrealProjectsList)
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

    public static void RefreshListBox(ListBox projectListBox)
    {
        List<UnrealProject> urealProjectsList = [];

        GetAllRecentlyOpenedProjects(urealProjectsList);
        SortByDate(urealProjectsList);

        projectListBox.ItemsSource = urealProjectsList;
    }

    public static string GetSelectedProjectPath(ListBox projectsListBox)
    {
        return projectsListBox.SelectedItem != null ? ((UnrealProject)projectsListBox.SelectedItem).UProjectPath : string.Empty;
    }

    public static ExecResult<(string, string)> GetUnrealProjectContextMenuFromRegedit()
    {
        if (!OperatingSystem.IsWindows())
        {
            return ExecResult<(string, string)>.Failed(ExecCode.SystemNotWindows);
        }

        const string keyPath1 = @"Unreal.ProjectFile\shell\rungenproj\command";
        const string keyPath2 = @"Unreal.ProjectFile\shell\switchversion\command";

        using var commandKey1 = Registry.ClassesRoot.OpenSubKey(keyPath1);
        using var commandKey2 = Registry.ClassesRoot.OpenSubKey(keyPath2);

        if (commandKey1 == null || commandKey2 == null)
        {
            return ExecResult<(string, string)>.Failed(ExecCode.RegeditNotFound);
        }

        var command1 = commandKey1.GetValue(null) as string ?? string.Empty;
        var command2 = commandKey2.GetValue(null) as string ?? string.Empty;

        return ExecResult<(string, string)>.Success((command1, command2));
    }

    public static ExecResult<string> GetInstalledUnrealFromRegedit()
    {
        if (!OperatingSystem.IsWindows())
        {
            return ExecResult<string>.Failed(ExecCode.SystemNotWindows);
        }

        const string keyPath = @"SOFTWARE\EpicGames\Unreal Engine";

        using var baseKey = Registry.LocalMachine.OpenSubKey(keyPath);

        if (baseKey == null)
        {
            return ExecResult<string>.Failed(ExecCode.RegeditNotFound);
        }

        // Sort the version number, always using the newest version
        var subKeys = baseKey.GetSubKeyNames();
        Array.Sort(subKeys, (a, b) => new Version(b).CompareTo(new Version(a)));

        // Get the UnrealEditor.exe path from the key value
        using var subKeyValue = Registry.LocalMachine.OpenSubKey(Path.Combine(keyPath, subKeys[0]));
        if (subKeyValue == null)
        {
            return ExecResult<string>.Failed(ExecCode.RegeditNotFound);
        }

        var installedDirectoryPath = subKeyValue.GetValue("InstalledDirectory") as string ?? string.Empty;
        var unrealEditorPath = Path.Combine(installedDirectoryPath, "Engine", "Binaries", "Win64", "UnrealEditor.exe");

        return ExecResult<string>.Success(unrealEditorPath);
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