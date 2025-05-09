using Avalonia.Controls;
using Avalonia.Input;

namespace UnrealLauncher;

public partial class MessageBox : Window
{
    public MessageBox()
    {
        InitializeComponent();
    }

    public MessageBox(string message)
    {
        InitializeComponent();

        ErrorMessage.Text = message;
    }

    private void InputElement_CloseWindow_OnTapped(object? sender, TappedEventArgs e)
    {
        Close();
    }
}