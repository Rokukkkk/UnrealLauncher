using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UnrealLauncher.Core;

namespace UnrealLauncher;

public class App : Application
{
    private readonly TrayIcon _trayIcon = new();
    private Dictionary<string, string>? _projectDic;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();

            // Setup tray icon
            _trayIcon.Clicked += TrayIcon_OnClick;
            _trayIcon.Icon = desktop.MainWindow.Icon;
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void RefreshTrayIcon(string[] projects)
    {
        _projectDic = new Dictionary<string, string>();
        foreach (var project in projects)
        {
            _projectDic.Add(Path.GetFileNameWithoutExtension(project), project);
        }

        var nativeMenu = new NativeMenu();
        _trayIcon.Menu = nativeMenu;

        foreach (var project in _projectDic)
        {
            var nativeMenuItem = new NativeMenuItem(project.Key);
            nativeMenuItem.Click += NativeMenuItem_Project_OnClick;
            nativeMenu.Items.Add(nativeMenuItem);
        }

        nativeMenu.Items.Add(new NativeMenuItemSeparator());
        var menuExit = new NativeMenuItem("Exit");
        menuExit.Click += NativeMenuItem_Exit_OnClick;
        nativeMenu.Items.Add(menuExit);
    }

    private void NativeMenuItem_Project_OnClick(object? sender, EventArgs e)
    {
        if (sender is not NativeMenuItem nativeMenuItem) return;
        if (nativeMenuItem.Header == null) return;

        var projectPath = _projectDic?[nativeMenuItem.Header];
        FileOps.Open(projectPath);
    }

    private void TrayIcon_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Show();
        }
    }

    private void NativeMenuItem_Exit_OnClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}