using Slippi.NET.Console.Types;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ComboRenderer;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private bool _initializing = true;
    private readonly FoxRenderer _parent;

    public SettingsWindow(FoxRenderer parent)
    {
        _parent = parent;
        this.Icon = _parent.Icon;

        InitializeComponent();

        this.chkFollowDolphin.IsChecked = SettingsManager.Instance.Settings.FollowDolphin;
        this.ConnectCodes.ItemsSource = SettingsManager.Instance.Settings.ConnectCodes;
        this.DisplayNames.ItemsSource = SettingsManager.Instance.Settings.DisplayNames;
        this.DolphinStatusText.Text = $"Dolphin status: {SettingsManager.Instance.DolphinConnectionStatus}";
        //this.ComboRendererStatusText.Text = "Combo renderer status: Active";
        this.WidthBox.Value = SettingsManager.Instance.Settings.ExplicitWidth;
        this.HeightBox.Value = SettingsManager.Instance.Settings.ExplicitHeight;

        SettingsManager.Instance.OnPausePlay += OnPausePlay;
        SettingsManager.Instance.OnDolphinConnectionStatus += OnDolphinConnectionStatus;

        _initializing = false;
    }

    protected override void OnActivated(EventArgs e)
    {
        InvalidateMeasure();
        base.OnActivated(e);
    }

    private void OnDolphinConnectionStatus(object? sender, ConnectionStatus e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            this.DolphinStatusText.Text = $"Dolphin status: {e}";
        });
    }

    private void OnPausePlay(object? sender, bool e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            //this.ComboRendererStatusText.Text = $"Combo renderer status: {(e ? "Active" : "Paused")}";
        });
    }

    private void HideButton_Click(object sender, RoutedEventArgs e)
    {
        _parent.ShowInTaskbar = false;
        _parent.Show();

        this.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        _parent.ShowInTaskbar = false;
        _parent.Show();
    }

    private void chkFollowDolphin_Checked(object sender, RoutedEventArgs e)
    {
        if (!_initializing)
        {
            SettingsManager.Instance.Settings.FollowDolphin = true;
            SettingsManager.Instance.SaveSettings();
        }
    }

    private void chkFollowDolphin_Unchecked(object sender, RoutedEventArgs e)
    {
        if (!_initializing)
        {
            SettingsManager.Instance.Settings.FollowDolphin = false;
            SettingsManager.Instance.SaveSettings();
        }
    }

    private void AddConnectCode_Click(object sender, RoutedEventArgs e)
    {
        TextInput connectInput = new TextInput();
        connectInput.ShowDialog();

        if (!string.IsNullOrEmpty(connectInput.InputText))
        {
            SettingsManager.Instance.Settings.ConnectCodes.Add(connectInput.InputText);
            SettingsManager.Instance.SaveSettings();

            this.ConnectCodes.ItemsSource = null;
            this.ConnectCodes.ItemsSource = SettingsManager.Instance.Settings.ConnectCodes;
        }
    }

    private void RemoveConnectCode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button element && element.DataContext is string connectCode)
        {
            SettingsManager.Instance.Settings.ConnectCodes.Remove(connectCode);
            SettingsManager.Instance.SaveSettings();

            this.ConnectCodes.ItemsSource = null;
            this.ConnectCodes.ItemsSource = SettingsManager.Instance.Settings.ConnectCodes;
        }
    }

    private void AddDisplayName_Click(object sender, RoutedEventArgs e)
    {
        TextInput connectInput = new TextInput();
        connectInput.ShowDialog();

        if (!string.IsNullOrEmpty(connectInput.InputText))
        {
            SettingsManager.Instance.Settings.DisplayNames.Add(connectInput.InputText);
            SettingsManager.Instance.SaveSettings();

            this.DisplayNames.ItemsSource = null;
            this.DisplayNames.ItemsSource = SettingsManager.Instance.Settings.DisplayNames;
        }
    }

    private void RemoveDisplayName_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button element && element.DataContext is string displayName)
        {
            SettingsManager.Instance.Settings.DisplayNames.Remove(displayName);
            SettingsManager.Instance.SaveSettings();

            this.DisplayNames.ItemsSource = null;
            this.DisplayNames.ItemsSource = SettingsManager.Instance.Settings.DisplayNames;
        }
    }

    private void PauseRendererButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsManager.Instance.IsPaused = true;
    }

    private void PlayRendererButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsManager.Instance.IsPaused = false;
    }

    private void TrackWindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            // 😈 
            SettingsManager.Instance.Settings.TrackWindow = (string)((ComboBoxItem)TrackWindowComboBox.SelectedItem).Content;
            SettingsManager.Instance.SaveSettings();
        }
    }

    private void WidthBox_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
    {
        if (!_initializing)
        {
            SettingsManager.Instance.Settings.ExplicitWidth = args.NewValue;
            SettingsManager.Instance.SaveSettings();
        }

    }

    private void HeightBox_ValueChanged(ModernWpf.Controls.NumberBox sender, ModernWpf.Controls.NumberBoxValueChangedEventArgs args)
    {
        if (!_initializing)
        {
            SettingsManager.Instance.Settings.ExplicitHeight = args.NewValue;
            SettingsManager.Instance.SaveSettings();
        }
    }
}
