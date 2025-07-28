using System.ComponentModel;
using System.Windows;

namespace ComboRenderer;

/// <summary>
/// Interaction logic for TextInput.xaml
/// </summary>
public partial class TextInput : Window
{
    public TextInput()
    {
        InitializeComponent();
    }

    public string InputText { get; set; } = string.Empty;

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        InputBox.Text = string.Empty;
        Close();
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        InputText = InputBox.Text;

        base.OnClosing(e);
    }
}
