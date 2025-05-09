using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using UnrealLauncher.Core;


namespace UnrealLauncher;

public class UnrealProject(string thumbnailPath, string uProjectPath)
{
    public Bitmap Thumbnail { get; } = thumbnailPath == Search.UnrealNoPicFoundImagePath ? new Bitmap(AssetLoader.Open(new Uri(thumbnailPath))) : new Bitmap(thumbnailPath);
    public string UProjectPath { get; } = uProjectPath;
    public string UProjectName { get; } = FileOps.GetFileNameWithoutExtension(uProjectPath);
    public string LastAccessDate { get; } = FileOps.GetLastAccessTime(uProjectPath);
}

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        RefreshListBox();
    }

    private void RefreshListBox()
    {
        List<UnrealProject> urealProjectsList = [];

        Search.GetAllRecentlyOpenedProjects(urealProjectsList);
        Search.SortByDate(urealProjectsList);

        ProjectsListBox.ItemsSource = urealProjectsList;
    }

    private string GetSelectedItemPath()
    {
        return ProjectsListBox.SelectedItem != null ? ((UnrealProject)ProjectsListBox.SelectedItem).UProjectPath : string.Empty;
    }

    private void ProjectsListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        FileOps.OpenProject(GetSelectedItemPath(), out var execCode);
        if (execCode != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode}");
        }

        ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_Open_OnClick(object? sender, RoutedEventArgs e)
    {
        FileOps.OpenProject(GetSelectedItemPath(), out var execCode);
        if (execCode != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode}");
        }
    }

    private void MenuItem_Delete_OnClick(object? sender, RoutedEventArgs e)
    {
        Overlay.Opacity = 1;
        Overlay.IsHitTestVisible = true;
    }

    private void MenuItem_OpenInExplorer_OnClick(object? sender, RoutedEventArgs e)
    {
        FileOps.OpenInExplorer(GetSelectedItemPath(), out var execCode);
        if (execCode != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode}");
        }

        ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_GenerateVS_OnClick(object? sender, RoutedEventArgs e)
    {
        var args = Search.GetUnrealProjectContextMenuFromRegedit(out var execCode1);
        if (execCode1 != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode1}");
            return;
        }

        var command = args.Item1;
        FileOps.OpenWithArgs(command, GetSelectedItemPath(), out var execCode2);
        if (execCode2 != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode2}");
        }
    }

    private void MenuItem_Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        FileOps.DeleteIntermediate(GetSelectedItemPath(), out var execCode);
        if (execCode != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode}");
        }

        RefreshListBox();
    }

    private void InputElement_Overlay_OnTapped(object? sender, TappedEventArgs e)
    {
        Overlay.Opacity = 0;
        Overlay.IsHitTestVisible = false;
    }

    private void InputElement_Messagebox_OnTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
    }

    private void InputElement_ButtonClose_OnTapped(object? sender, TappedEventArgs e)
    {
        Overlay.Opacity = 0;
        Overlay.IsHitTestVisible = false;
    }

    private void InputElement_ButtonDelete_OnTapped(object? sender, TappedEventArgs e)
    {
        FileOps.DeleteProject(GetSelectedItemPath(), out var execCode);
        if (execCode != ExecCode.Success)
        {
            _ = PopMessageBox($"execCode: {execCode}");
        }

        Overlay.Opacity = 0;
        Overlay.IsHitTestVisible = false;

        RefreshListBox();
    }

    private void InputElement_Refresh_OnTapped(object? sender, TappedEventArgs e)
    {
        RefreshListBox();
    }

    private async Task PopMessageBox(string message)
    {
        var messageBox = new MessageBox(message);
        await messageBox.ShowDialog(this);
    }
}