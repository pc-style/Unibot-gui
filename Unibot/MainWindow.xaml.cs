using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Unibot.Models;
using Unibot.Core;

namespace Unibot;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private Configuration _configuration;
    private bool _isRunning;
    private UnibotEngine? _engine;

    public MainWindow()
    {
        InitializeComponent();
        Configuration = new Configuration();
        DataContext = this;
        
        // Initialize with default values
        LoadDefaultConfiguration();
    }

    public Configuration Configuration
    {
        get => _configuration;
        set
        {
            _configuration = value;
            OnPropertyChanged();
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged();
            UpdateButtonStates();
        }
    }

    private void LoadDefaultConfiguration()
    {
        // Load default configuration values (matching the original Python config)
        Configuration.Screen.ColorRange.Lower = new HsvColor(58, 210, 80);
        Configuration.Screen.ColorRange.Upper = new HsvColor(63, 255, 255);
        Configuration.Screen.FovX = 256;
        Configuration.Screen.FovY = 256;
        Configuration.Screen.AimFovX = 256;
        Configuration.Screen.AimFovY = 256;
        Configuration.Screen.Fps = 60;
        Configuration.Screen.AutoDetectResolution = true;
        Configuration.Screen.ResolutionX = 1920;
        Configuration.Screen.ResolutionY = 1080;
        Configuration.Screen.DetectionThresholdX = 3;
        Configuration.Screen.DetectionThresholdY = 3;

        Configuration.Aim.Speed = 1.0;
        Configuration.Aim.YSpeed = 1.0;
        Configuration.Aim.Smooth = 0.0;
        Configuration.Aim.AimHeight = 0.5;
        Configuration.Aim.Offset = 0;

        Configuration.Recoil.Mode = RecoilMode.Move;
        Configuration.Recoil.RecoilX = 0.0;
        Configuration.Recoil.RecoilY = 0.0;
        Configuration.Recoil.MaxOffset = 100;
        Configuration.Recoil.Recover = 0.0;

        Configuration.Trigger.TriggerDelay = 0;
        Configuration.Trigger.TriggerRandomization = 30;
        Configuration.Trigger.TriggerThreshold = 8;

        Configuration.RapidFire.TargetCps = 10;

        Configuration.Communication.Type = CommunicationType.None;
        Configuration.Communication.Ip = "0.0.0.0";
        Configuration.Communication.Port = 50124;
        Configuration.Communication.ComPort = "COM1";

        Configuration.Debug.Enabled = true;
        Configuration.Debug.AlwaysOn = true;
        Configuration.Debug.DisplayMode = DisplayMode.Mask;
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IsRunning)
        {
            StartUnibot();
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsRunning)
        {
            StopUnibot();
        }
    }

    private void StartUnibot()
    {
        try
        {
            _engine?.Dispose(); // Clean up any existing engine
            _engine = new UnibotEngine(Configuration);
            _engine.Start();
            IsRunning = true;
            ShowStatusMessage("Unibot started successfully", MessageType.Success);
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to start Unibot: {ex.Message}", MessageType.Error);
        }
    }

    private void StopUnibot()
    {
        try
        {
            _engine?.Stop();
            _engine?.Dispose();
            _engine = null;
            IsRunning = false;
            ShowStatusMessage("Unibot stopped", MessageType.Info);
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Error stopping Unibot: {ex.Message}", MessageType.Error);
        }
    }

    private void UpdateButtonStates()
    {
        if (StartButton != null && StopButton != null)
        {
            StartButton.IsEnabled = !IsRunning;
            StopButton.IsEnabled = IsRunning;
        }
    }

    private void ShowStatusMessage(string message, MessageType type)
    {
        // TODO: Implement status message display
        System.Diagnostics.Debug.WriteLine($"[{type}] {message}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnClosed(EventArgs e)
    {
        StopUnibot();
        base.OnClosed(e);
    }

    private enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }
}