using Newtonsoft.Json;
using Slippi.NET.Console.Types;
using System;
using System.Collections.Generic;
using System.IO;

namespace ComboRenderer;

internal class SettingsManager
{
    public static SettingsManager Instance = new SettingsManager();

    private readonly string _settingsPath;
    private readonly ComboRendererSettings _settings;

    private SettingsManager()
    {
        string settingsFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlippiComboRenderer");
        Directory.CreateDirectory(settingsFolder);

        _settingsPath = Path.Join(settingsFolder, "settings.json");
        if (File.Exists(_settingsPath) && 
            File.ReadAllText(_settingsPath) is string settingsData &&
            JsonConvert.DeserializeObject<ComboRendererSettings>(settingsData) is ComboRendererSettings existingSettings)
        {
            _settings = existingSettings;
            IsFirstLaunch = false;
        }
        else
        {
            IsFirstLaunch = true;
            _settings = new ComboRendererSettings();
            SaveSettings();
        }
    }

    public ComboRendererSettings Settings => _settings;
    public bool IsFirstLaunch { get; }
    public bool IsPaused
    {
        get;
        set
        {
            if (field != value)
            {
                OnPausePlay?.Invoke(this, !value);
                field = value;
            }
        }
    } = false;
    public event EventHandler<bool>? OnPausePlay;

    public ConnectionStatus DolphinConnectionStatus
    {
        get;
        set
        {
            if (field != value)
            {
                OnDolphinConnectionStatus?.Invoke(this, value);
                field = value;
            }
        }
    } = ConnectionStatus.Disconnected;
    public event EventHandler<ConnectionStatus>? OnDolphinConnectionStatus;

    public void SaveSettings()
    {
        File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(_settings));
    }
}
