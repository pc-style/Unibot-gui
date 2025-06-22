using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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
    private string _currentPage = "Detection";

    public MainWindow()
    {
        InitializeComponent();
        Configuration = new Configuration();
        DataContext = this;
        
        // Initialize with default values
        LoadDefaultConfiguration();
        
        // Enable window dragging
        MouseLeftButtonDown += (sender, e) => DragMove();
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

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string page)
        {
            SwitchToPage(page);
        }
    }

    private void SwitchToPage(string pageName)
    {
        // Update current page
        _currentPage = pageName;

        // Hide all panels
        DetectionPanel.Visibility = Visibility.Collapsed;
        AimPanel.Visibility = Visibility.Collapsed;
        RecoilPanel.Visibility = Visibility.Collapsed;
        TriggerPanel.Visibility = Visibility.Collapsed;
        CommunicationPanel.Visibility = Visibility.Collapsed;
        DebugPanel.Visibility = Visibility.Collapsed;

        // Reset all navigation button styles
        DetectionNavButton.Style = FindResource("NavigationButtonStyle") as Style;
        AimNavButton.Style = FindResource("NavigationButtonStyle") as Style;
        RecoilNavButton.Style = FindResource("NavigationButtonStyle") as Style;
        TriggerNavButton.Style = FindResource("NavigationButtonStyle") as Style;
        CommunicationNavButton.Style = FindResource("NavigationButtonStyle") as Style;
        DebugNavButton.Style = FindResource("NavigationButtonStyle") as Style;

        // Show selected panel and update navigation
        switch (pageName)
        {
            case "Detection":
                DetectionPanel.Visibility = Visibility.Visible;
                DetectionNavButton.Style = FindResource("ActiveNavigationButtonStyle") as Style;
                PageTitleText.Text = "Detection & Targeting";
                PageSubtitleText.Text = "Configure color detection and field of view settings";
                break;
            case "Aim":
                AimPanel.Visibility = Visibility.Visible;
                AimNavButton.Style = FindResource("ActiveNavigationButtonStyle") as Style;
                PageTitleText.Text = "Aim Settings";
                PageSubtitleText.Text = "Configure aim assistance parameters and sensitivity";
                break;
            case "Recoil":
                RecoilPanel.Visibility = Visibility.Visible;
                RecoilNavButton.Style = FindResource("ActiveNavigationButtonStyle") as Style;
                PageTitleText.Text = "Recoil Control";
                PageSubtitleText.Text = "Configure recoil compensation and movement settings";
                break;
            case "Trigger":
                TriggerPanel.Visibility = Visibility.Visible;
                TriggerNavButton.Style = FindResource("ActiveNavigationButtonStyle") as Style;
                PageTitleText.Text = "Triggerbot";
                PageSubtitleText.Text = "Configure automatic trigger and rapid fire settings";
                break;
            case "Communication":
                CommunicationPanel.Visibility = Visibility.Visible;
                CommunicationNavButton.Style = FindResource("ActiveNavigationButtonStyle") as Style;
                PageTitleText.Text = "Communication";
                PageSubtitleText.Text = "Configure input methods and hardware communication";
                break;
            case "Debug":
                DebugPanel.Visibility = Visibility.Visible;
                DebugNavButton.Style = FindResource("ActiveNavigationButtonStyle") as Style;
                PageTitleText.Text = "Debug & Preview";
                PageSubtitleText.Text = "Debug settings and live preview functionality";
                break;
        }
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

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
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
            
            // Update button content and styles based on state
            if (IsRunning)
            {
                StartButton.Content = "► Running...";
                StopButton.Content = "■ Stop Unibot";
                StopButton.Style = FindResource("ModernButtonStyle") as Style;
                StopButton.Background = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
            }
            else
            {
                StartButton.Content = "► Start Unibot";
                StopButton.Content = "■ Stop Unibot";
                StopButton.Style = FindResource("ModernSecondaryButtonStyle") as Style;
            }
        }
    }

    private void ShowStatusMessage(string message, MessageType type)
    {
        // TODO: Implement status message display (could be a toast notification or status bar)
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