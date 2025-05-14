using System;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;

namespace UnrealLauncher.Core;

public static partial class AssetManagement
{
    private static readonly string EpicAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EpicGamesLauncher");
    private const string EpicSettingsPath = "Saved/Config/Windows/GameUserSettings.ini";

    public static void RefreshAssetsListBox(ListBox assetsListBox)
    {
        var vaultCacheDirectory = GetVaultCacheDirectory();
        assetsListBox.ItemsSource = Directory.GetDirectories(vaultCacheDirectory);
    }

    public static string GetVaultCacheDirectory()
    {
        var vaultCacheDirectory = string.Empty;
        var regex = FindDirectory();
        var file = Path.Combine(EpicAppDataPath, EpicSettingsPath);

        using var reader = new StreamReader(file);
        while (reader.ReadLine() is { } line)
        {
            var match = regex.Match(line);
            if (!match.Success) continue;

            vaultCacheDirectory = match.Groups[1].Value;
        }

        return vaultCacheDirectory;
    }

    [GeneratedRegex("VaultCacheDirectories=(.+)$")]
    private static partial Regex FindDirectory();
}