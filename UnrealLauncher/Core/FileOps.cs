using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UnrealLauncher.Core;

public static class FileOps
{
    public static ExecResult<Void> OpenProject(string? path)
    {
        if (path == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        Process.Start(new ProcessStartInfo(path)
        {
            UseShellExecute = true
        });

        return ExecResult<Void>.Success();
    }

    public static ExecResult<Void> OpenInExplorer(string? path)
    {
        if (path == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path.Replace('/', '\\')}\"")
        {
            UseShellExecute = true
        });

        return ExecResult<Void>.Success();
    }

    public static ExecResult<Void> OpenWithArgs(string? command, string unrealProjectPath)
    {
        if (command == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        var firstQuoteEnd = command.IndexOf('"', 1);
        var exePath = command.Substring(1, firstQuoteEnd - 1);
        var argumentsTemplate = command[(firstQuoteEnd + 1)..].Trim();

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = argumentsTemplate.Replace("%1", unrealProjectPath),
            UseShellExecute = false
        });

        return ExecResult<Void>.Success();
    }

    public static ExecResult<Void> OpenWithOutArgs(string? command)
    {
        if (command == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = command,
            UseShellExecute = true
        });

        return ExecResult<Void>.Success();
    }

    public static ExecResult<Void> DeleteProject(string? path)
    {
        if (path == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        if (Process.GetProcessesByName("UnrealEditor").Length != 0)
        {
            return ExecResult<Void>.Failed(ExecCode.UnrealIsRunning);
        }

        var directoryPath = Path.GetDirectoryName(path);
        if (directoryPath == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        try
        {
            Directory.Delete(directoryPath, recursive: true);
        }
        catch (FileNotFoundException)
        {
            return ExecResult<Void>.Failed(ExecCode.FileNotFound);
        }
        catch (UnauthorizedAccessException)
        {
            return ExecResult<Void>.Failed(ExecCode.FileAccessDenied);
        }
        catch (IOException)
        {
            return ExecResult<Void>.Failed(ExecCode.FileIsOccupying);
        }

        return ExecResult<Void>.Success();
    }

    public static ExecResult<Void> DeleteIntermediate(string? path)
    {
        if (path == null)
        {
            return ExecResult<Void>.Failed(ExecCode.PathIsNull);
        }

        if (Process.GetProcessesByName("UnrealEditor").Length != 0)
        {
            return ExecResult<Void>.Failed(ExecCode.UnrealIsRunning);
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

        return hasError ? ExecResult<Void>.Failed(ExecCode.FileNotFound) : ExecResult<Void>.Success();
    }

    public static void HandleOpenFolder(string? path)
    {
        if (path == null) return;

        var projectName = Path.GetFileName(Path.GetDirectoryName(path));
        Process.Start(new ProcessStartInfo(Path.Combine(path, projectName + ".uproject"))
        {
            UseShellExecute = true
        });
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