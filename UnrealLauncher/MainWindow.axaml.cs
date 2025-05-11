using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using UnrealLauncher.Core;

namespace UnrealLauncher;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Search.RefreshListBox(ProjectsListBox);
    }

    private void ProjectsListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        Try(FileOps.OpenProject(Search.GetSelectedProjectPath(ProjectsListBox)), out _);

        ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_Open_OnClick(object? sender, RoutedEventArgs e)
    {
        Try(FileOps.OpenProject(Search.GetSelectedProjectPath(ProjectsListBox)), out _);

        ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_Delete_OnClick(object? sender, RoutedEventArgs e)
    {
        Overlay.Opacity = 1;
        Overlay.IsHitTestVisible = true;
    }

    private void MenuItem_OpenInExplorer_OnClick(object? sender, RoutedEventArgs e)
    {
        Try(FileOps.OpenInExplorer(Search.GetSelectedProjectPath(ProjectsListBox)), out _);

        ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_GenerateVS_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!Try(Search.GetUnrealProjectContextMenuFromRegedit(), out var value))
        {
            return;
        }

        var command = value.Item1;

        Try(FileOps.OpenWithArgs(command, Search.GetSelectedProjectPath(ProjectsListBox)), out _);
    }

    private void MenuItem_SwitchUEVersion_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!Try(Search.GetUnrealProjectContextMenuFromRegedit(), out var value))
        {
            return;
        }

        var command = value.Item2;

        Try(FileOps.OpenWithArgs(command, Search.GetSelectedProjectPath(ProjectsListBox)), out _);
    }

    private void MenuItem_Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        Try(FileOps.DeleteIntermediate(Search.GetSelectedProjectPath(ProjectsListBox)), out _);

        Search.RefreshListBox(ProjectsListBox);
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
        Try(FileOps.DeleteProject(Search.GetSelectedProjectPath(ProjectsListBox)), out _);

        Overlay.Opacity = 0;
        Overlay.IsHitTestVisible = false;

        Search.RefreshListBox(ProjectsListBox);
    }

    private void InputElement_Refresh_OnTapped(object? sender, TappedEventArgs e)
    {
        Search.RefreshListBox(ProjectsListBox);
    }

    private void InputElement_AddProject_OnTapped(object? sender, TappedEventArgs e)
    {
        WindowHelper.OpenFolderPickerAsync(this, FileOps.HandleOpenFolder);
    }

    private void InputElement_NewProject_OnTapped(object? sender, TappedEventArgs e)
    {
        if (!Try(Search.GetInstalledUnrealFromRegedit(), out var value))
        {
            return;
        }

        Try(FileOps.OpenWithOutArgs(value), out _);
    }

    private bool Try<T>(ExecResult<T> result, out T value)
    {
        if (!result.IsSuccess)
        {
            _ = WindowHelper.PopMessageBox(this, $"ErrCode: {result.ExecCode}");
            value = default!;
            return false;
        }

        value = result.Data;
        return true;
    }
}