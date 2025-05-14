using Avalonia.Controls;
using Avalonia.Input;
using UnrealLauncher.Core;

namespace UnrealLauncher.Views;

public partial class AssetsView : UserControl
{
    public AssetsView()
    {
        InitializeComponent();

        AssetManagement.RefreshAssetsListBox(AssetsListBox);
    }

    private void InputElement_OpenVaultCacheDirectory_OnTapped(object? sender, TappedEventArgs e)
    {
        FileOps.OpenInExplorer(AssetManagement.GetVaultCacheDirectory());
    }
}