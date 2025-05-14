using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace UnrealLauncher.Core;

public static class WindowHelper
{
    public static bool Try<T>(Window window, ExecResult<T> result, out T value)
    {
        if (!result.IsSuccess)
        {
            _ = PopMessageBox(window, $"ErrCode: {result.ExecCode}");
            value = default!;
            return false;
        }

        value = result.Data;
        return true;
    }

    private static async Task PopMessageBox(Window window, string message)
    {
        var messageBox = new MessageBox(message);
        await messageBox.ShowDialog(window);
    }

    public static void OpenFolderPickerAsync(Control control, Action<string?>? onFolderPicked = null, string title = "Please choose a project folder")
    {
        _ = FireAndForgetPickFolderAsync(control, title, onFolderPicked);
        return;

        static async Task FireAndForgetPickFolderAsync(Control control, string title, Action<string?>? onFolderPicked)
        {
            var window = control.GetVisualRoot() as Window;

            try
            {
                var topLevel = TopLevel.GetTopLevel(control);
                if (topLevel?.StorageProvider == null)
                {
                    if (window != null) _ = PopMessageBox(window, "Storage provider not available.");
                    return;
                }

                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false
                });

                var folderPath = folders.Count > 0 ? folders[0].Path.LocalPath : null;
                onFolderPicked?.Invoke(folderPath);
            }
            catch (Exception ex)
            {
                if (window != null) _ = PopMessageBox(window, ex.Message);
                onFolderPicked?.Invoke(null);
            }
        }
    }
}