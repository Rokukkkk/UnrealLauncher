using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace UnrealLauncher.Views;

public partial class ProjectsView : UserControl
{
    public event EventHandler<TappedEventArgs>? AddProjectOnTapped;
    public event EventHandler<TappedEventArgs>? NewProjectOnTapped;
    public event EventHandler<TappedEventArgs>? ProjectsListBoxOnDoubleTapped;
    public event EventHandler<RoutedEventArgs>? MenuItemOpenOnClick;
    public event EventHandler<RoutedEventArgs>? MenuItemOpenInExplorerOnClick;
    public event EventHandler<RoutedEventArgs>? MenuItemGenerateVsOnClick;
    public event EventHandler<RoutedEventArgs>? MenuItemSwitchUeVersionOnClick;
    public event EventHandler<RoutedEventArgs>? MenuItemClearOnClick;
    public event EventHandler<RoutedEventArgs>? MenuItemDeleteOnClick;

    public ProjectsView()
    {
        InitializeComponent();
    }

    private void InputElement_AddProject_OnTapped(object? sender, TappedEventArgs e)
    {
        AddProjectOnTapped?.Invoke(sender, e);
    }

    private void InputElement_NewProject_OnTapped(object? sender, TappedEventArgs e)
    {
        NewProjectOnTapped?.Invoke(sender, e);
    }

    private void ProjectsListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        ProjectsListBoxOnDoubleTapped?.Invoke(sender, e);
    }

    private void MenuItem_Open_OnClick(object? sender, RoutedEventArgs e)
    {
        MenuItemOpenOnClick?.Invoke(sender, e);
    }

    private void MenuItem_OpenInExplorer_OnClick(object? sender, RoutedEventArgs e)
    {
        MenuItemOpenInExplorerOnClick?.Invoke(sender, e);
    }

    private void MenuItem_GenerateVS_OnClick(object? sender, RoutedEventArgs e)
    {
        MenuItemGenerateVsOnClick?.Invoke(sender, e);
    }

    private void MenuItem_SwitchUEVersion_OnClick(object? sender, RoutedEventArgs e)
    {
        MenuItemSwitchUeVersionOnClick?.Invoke(sender, e);
    }

    private void MenuItem_Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        MenuItemClearOnClick?.Invoke(sender, e);
    }

    private void MenuItem_Delete_OnClick(object? sender, RoutedEventArgs e)
    {
        MenuItemDeleteOnClick?.Invoke(sender, e);
    }
}