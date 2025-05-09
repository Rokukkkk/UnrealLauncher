using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UnrealLauncher.Core;

public static class FileOps
{
    public static void OpenProject(string? path, out ExecCode execCode)
    {
        if (path == null)
        {
            execCode = ExecCode.PathIsNull;
            return;
        }

        execCode = ExecCode.Success;
        Process.Start(new ProcessStartInfo(path)
        {
            UseShellExecute = true
        });
    }

    public static void OpenInExplorer(string? path, out ExecCode execCode)
    {
        if (path == null)
        {
            execCode = ExecCode.PathIsNull;
            return;
        }

        execCode = ExecCode.Success;
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path.Replace('/', '\\')}\"")
        {
            UseShellExecute = true
        });
    }

    public static void OpenWithArgs(string? command, string unrealProjectPath, out ExecCode execCode)
    {
        if (command == null)
        {
            execCode = ExecCode.PathIsNull;
            return;
        }

        var firstQuoteEnd = command.IndexOf('"', 1);
        var exePath = command.Substring(1, firstQuoteEnd - 1);
        var argumentsTemplate = command[(firstQuoteEnd + 1)..].Trim();

        execCode = ExecCode.Success;
        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = argumentsTemplate.Replace("%1", unrealProjectPath),
            UseShellExecute = false
        });
    }

    public static void DeleteProject(string? path, out ExecCode execCode)
    {
        if (path == null)
        {
            execCode = ExecCode.PathIsNull;
            return;
        }

        if (Process.GetProcessesByName("UnrealEditor").Length != 0)
        {
            execCode = ExecCode.UEisRunning;
            return;
        }

        var directoryPath = Path.GetDirectoryName(path);
        if (directoryPath == null)
        {
            execCode = ExecCode.PathIsNull;
            return;
        }

        try
        {
            Directory.Delete(directoryPath, recursive: true);
        }
        catch (IOException)
        {
            execCode = ExecCode.FileOccupying;
            return;
        }

        execCode = ExecCode.Success;
    }

    public static void DeleteIntermediate(string? path, out ExecCode execCode)
    {
        if (path == null)
        {
            execCode = ExecCode.PathIsNull;
            return;
        }

        if (Process.GetProcessesByName("UnrealEditor").Length != 0)
        {
            execCode = ExecCode.UEisRunning;
            return;
        }

        var directoryPath = Path.GetDirectoryName(path);
        var derivedDataCachePath = Path.Combine(directoryPath!, "DerivedDataCache");
        var intermediatePath = Path.Combine(directoryPath!, "Intermediate");
        var savedPath = Path.Combine(directoryPath!, "Saved");

        var hasError = false;

        try
        {
            Directory.Delete(derivedDataCachePath, recursive: true);
        }
        catch (IOException)
        {
            hasError = true;
        }

        try
        {
            Directory.Delete(intermediatePath, recursive: true);
        }
        catch (IOException)
        {
            hasError = true;
        }

        try
        {
            Directory.Delete(savedPath, recursive: true);
        }
        catch (IOException)
        {
            hasError = true;
        }

        if (hasError)
        {
            execCode = ExecCode.FileNotFound;
            return;
        }

        execCode = ExecCode.Success;
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static string GetLastAccessTime(string path)
    {
        return File.GetLastAccessTime(path).ToString("yyyy/MM/dd");
    }

    public static bool IsFileExists(string path)
    {
        return File.Exists(path);
    }

    public static IEnumerable<string> ReadLines(string path)
    {
        return File.ReadLines(path);
    }
}