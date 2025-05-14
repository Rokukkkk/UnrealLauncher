using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using UnrealLauncher.Core;
using UnrealLauncher.Views;

namespace UnrealLauncher;

public partial class MainWindow : Window
{
    private readonly ProjectsView _projectsView;
    private readonly AssetsView _assetsView;

    public MainWindow()
    {
        InitializeComponent();
        _projectsView = new ProjectsView();
        _assetsView = new AssetsView();

        ContentArea.Content = _projectsView;
        BindDelegates();

        Search.RefreshListBox(_projectsView.ProjectsListBox);
    }

    private void ProjectsListBox_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        WindowHelper.Try(this, FileOps.Open(Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);

        _projectsView.ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_Open_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowHelper.Try(this, FileOps.Open(Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);

        _projectsView.ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_Delete_OnClick(object? sender, RoutedEventArgs e)
    {
        Overlay.Opacity = 1;
        Overlay.IsHitTestVisible = true;
    }

    private void MenuItem_OpenInExplorer_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowHelper.Try(this, FileOps.OpenInExplorer(Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);

        _projectsView.ProjectsListBox.SelectedItem = null;
    }

    private void MenuItem_GenerateVS_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!WindowHelper.Try(this, Search.GetUnrealProjectContextMenuFromRegedit(), out var value))
        {
            return;
        }

        var command = value.Item1;

        WindowHelper.Try(this, FileOps.OpenWithArgs(command, Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);
    }

    private void MenuItem_SwitchUEVersion_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!WindowHelper.Try(this, Search.GetUnrealProjectContextMenuFromRegedit(), out var value))
        {
            return;
        }

        var command = value.Item2;

        WindowHelper.Try(this, FileOps.OpenWithArgs(command, Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);
    }

    private void MenuItem_Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowHelper.Try(this, FileOps.DeleteIntermediate(Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);

        Search.RefreshListBox(_projectsView.ProjectsListBox);
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
        WindowHelper.Try(this, FileOps.DeleteProject(Search.GetSelectedProjectPath(_projectsView.ProjectsListBox)), out _);

        Overlay.Opacity = 0;
        Overlay.IsHitTestVisible = false;

        Search.RefreshListBox(_projectsView.ProjectsListBox);
    }

    private void InputElement_Refresh_OnTapped(object? sender, TappedEventArgs e)
    {
        if (Tabs.SelectedItem is not ListBoxItem selectedItem)
            return;

        var tag = selectedItem.Tag?.ToString();

        switch (tag)
        {
            case "Projects":
                Search.RefreshListBox(_projectsView.ProjectsListBox);
                break;
            case "Assets":
                AssetManagement.RefreshAssetsListBox(_assetsView.AssetsListBox);
                break;
        }
    }

    private void InputElement_AddProject_OnTapped(object? sender, TappedEventArgs e)
    {
        WindowHelper.OpenFolderPickerAsync(this, FileOps.HandleOpenFolder);
    }

    private void InputElement_NewProject_OnTapped(object? sender, TappedEventArgs e)
    {
        if (!WindowHelper.Try(this, Search.GetInstalledUnrealFromRegedit(), out var value))
        {
            return;
        }

        WindowHelper.Try(this, FileOps.OpenWithOutArgs(value), out _);
    }

    private void InputElement_Version_OnTapped(object? sender, TappedEventArgs e)
    {
        FileOps.Open("https://github.com/Rokukkkk/UnrealLauncher");
    }

    private void Tabs_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Tabs.SelectionChanged += Tabs_SelectionChanged;
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
        if (Tabs.SelectedItem is not ListBoxItem selectedItem)
            return;

        var tag = selectedItem.Tag?.ToString();

        ContentArea.Content = tag switch
        {
            "Projects" => _projectsView,
            "Assets" => _assetsView,
            _ => null
        };
    }


    private void BindDelegates()
    {
        _projectsView.AddProjectOnTapped += InputElement_AddProject_OnTapped;
        _projectsView.NewProjectOnTapped += InputElement_NewProject_OnTapped;
        _projectsView.ProjectsListBoxOnDoubleTapped += ProjectsListBox_OnDoubleTapped;
        _projectsView.MenuItemOpenOnClick += MenuItem_Open_OnClick;
        _projectsView.MenuItemOpenInExplorerOnClick += MenuItem_OpenInExplorer_OnClick;
        _projectsView.MenuItemGenerateVsOnClick += MenuItem_GenerateVS_OnClick;
        _projectsView.MenuItemSwitchUeVersionOnClick += MenuItem_SwitchUEVersion_OnClick;
        _projectsView.MenuItemClearOnClick += MenuItem_Clear_OnClick;
        _projectsView.MenuItemDeleteOnClick += MenuItem_Delete_OnClick;
    }
}