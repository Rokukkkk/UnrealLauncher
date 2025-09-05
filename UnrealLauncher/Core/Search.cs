using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace UnrealLauncher.Core;

public static partial class Search
{
    public class UnrealProject(string thumbnailPath, string uProjectPath, string lastAccessDate)
    {
        public string ThumbnailPath { get; } = thumbnailPath;
        public string UProjectPath { get; } = uProjectPath;
        public string UProjectName { get; } = FileOps.GetFileNameWithoutExtension(uProjectPath);
        public string LastAccessDate { get; } = lastAccessDate;
    }

    private static readonly string UnrealAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UnrealEngine");
    private const string UnrealEditorSettingsPath = "Saved/Config/WindowsEditor/EditorSettings.ini";
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

    private static void GetAllRecentlyOpenedProjects(List<UnrealProject> unrealProjectsList)
    {
        const string searchString = "RecentlyOpenedProjectFiles";
        const string dateFormat = "yyyy.MM.dd-HH.mm.ss";
        var entries = new List<(string ProjectName, DateTime LastOpenTime)>();

        // Get config location
        var configPathList = new List<string>();
        configPathList.AddRange(
            GetEngineVersionList()
                .Select(dir => Path.Combine(dir, UnrealEditorSettingsPath))
                .Where(FileOps.IsFileExists)
        );

        var regex = FindNameAndDate();

        // Read files
        Parallel.ForEach(configPathList, file =>
        {
            using var reader = new StreamReader(file);
            while (reader.ReadLine() is { } line)
            {
                if (!line.Contains(searchString)) continue;

                var match = regex.Match(line);
                if (!match.Success) continue;

                lock (entries)
                {
                    entries.Add((match.Groups[1].Value, DateTime.ParseExact(match.Groups[2].Value, dateFormat, null)));
                }
            }
        });

        // Sort by last updated date
        var latestProjects = entries
            .GroupBy(e => e.ProjectName)
            .Select(g => g.OrderByDescending(e => e.LastOpenTime).First())
            .Where(e => FileOps.IsFileExists(e.ProjectName)) 
            .OrderByDescending(e => e.LastOpenTime) 
            .ToList();

        // Refresh tray menu
        var projects = latestProjects.Select(e => e.ProjectName).ToArray();
        var app = Application.Current as App;
        app?.RefreshTrayIcon(projects);

        // Update unrealProjectsList
        unrealProjectsList.AddRange(
            latestProjects.Select(e =>
                new UnrealProject(
                    GetAutoScreenshotPath(e.ProjectName),
                    e.ProjectName,
                    e.LastOpenTime.ToString("yyyy/MM/dd")
                )
            )
        );
    }


    public static void RefreshListBox(ListBox? projectListBox)
    {
        List<UnrealProject> urealProjectsList = [];

        GetAllRecentlyOpenedProjects(urealProjectsList);

        if (projectListBox != null) projectListBox.ItemsSource = urealProjectsList;
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

        var commandKey1 = Platforms.WinOps.GetRegistryClassesRoot(keyPath1);
        var commandKey2 = Platforms.WinOps.GetRegistryClassesRoot(keyPath2);

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

        var baseKey = Platforms.WinOps.GetRegistryLocalMachine(keyPath);

        if (baseKey == null)
        {
            return ExecResult<string>.Failed(ExecCode.RegeditNotFound);
        }

        // Sort the version number, always using the newest version
        var subKeys = baseKey.GetSubKeyNames();
        Array.Sort(subKeys, (a, b) => new Version(b).CompareTo(new Version(a)));

        // Get the UnrealEditor.exe path from the key value
        var subKeyValue = Platforms.WinOps.GetRegistryLocalMachine(Path.Combine(keyPath, subKeys[0]));
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
                    ProjectName="([^"]+)",LastOpenTime=([^)]+)
                    """)]
    private static partial Regex FindNameAndDate();
}