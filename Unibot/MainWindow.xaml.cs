using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.Json;
using Unibot.Models;
using Unibot.Core;

namespace Unibot;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private Configuration _configuration = null!;
    private bool _isRunning;
    private UnibotEngine? _engine;
    private string _currentPage = "Detection";
    private bool _isPreviewRunning;
    private System.Windows.Threading.DispatcherTimer? _previewTimer;
    private ScreenCapture? _previewCapture;

    // Status tracking
    private bool _aimActive;
    private bool _triggerActive;
    private bool _recoilActive;
    private bool _rapidFireActive;
    
    public string[] AvailableComPorts { get; private set; } = [];

    public MainWindow()
    {
        InitializeComponent();
        Configuration = new Configuration();
        DataContext = this;
        
        // Initialize with default values
        LoadDefaultConfiguration();

        // Load available COM ports for the dropdown
        LoadAvailableComPorts();
        
        // Try to load saved configuration
        LoadConfiguration();
        
        // Enable window dragging
        MouseLeftButtonDown += (sender, e) => DragMove();
        
        // Setup real-time updates
        SetupStatusUpdates();
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
            
            // Subscribe to engine events for real-time status updates
            _engine.AimStateChanged += (sender, active) => 
            {
                Dispatcher.Invoke(() => 
                {
                    _aimActive = active;
                    UpdateStatusIndicators();
                });
            };
            
            _engine.TriggerStateChanged += (sender, active) => 
            {
                Dispatcher.Invoke(() => 
                {
                    _triggerActive = active;
                    UpdateStatusIndicators();
                });
            };
            
            _engine.RecoilStateChanged += (sender, active) => 
            {
                Dispatcher.Invoke(() => 
                {
                    _recoilActive = active;
                    UpdateStatusIndicators();
                });
            };
            
            _engine.RapidFireStateChanged += (sender, active) => 
            {
                Dispatcher.Invoke(() => 
                {
                    _rapidFireActive = active;
                    UpdateStatusIndicators();
                });
            };
            
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
            
            // Reset status indicators
            _aimActive = false;
            _triggerActive = false;
            _recoilActive = false;
            _rapidFireActive = false;
            UpdateStatusIndicators();
            
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

    private void SetupStatusUpdates()
    {
        // Setup a timer to update status indicators every 100ms
        var statusTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        statusTimer.Tick += (sender, e) => UpdateStatusIndicators();
        statusTimer.Start();
    }

    private void UpdateStatusIndicators()
    {
        // Update aim status
        if (AimStatusIndicator != null && AimStatusText != null)
        {
            if (_aimActive)
            {
                AimStatusIndicator.Fill = FindResource("SecondaryAccentBrush") as System.Windows.Media.Brush;
                AimStatusText.Text = "Active";
                AimStatusText.Foreground = FindResource("SecondaryAccentBrush") as System.Windows.Media.Brush;
            }
            else
            {
                AimStatusIndicator.Fill = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
                AimStatusText.Text = "Inactive";
                AimStatusText.Foreground = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
            }
        }

        // Update recoil status
        if (RecoilStatusIndicator != null && RecoilStatusText != null)
        {
            if (_recoilActive)
            {
                RecoilStatusIndicator.Fill = FindResource("SecondaryAccentBrush") as System.Windows.Media.Brush;
                RecoilStatusText.Text = "Active";
                RecoilStatusText.Foreground = FindResource("SecondaryAccentBrush") as System.Windows.Media.Brush;
            }
            else
            {
                RecoilStatusIndicator.Fill = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
                RecoilStatusText.Text = "Inactive";
                RecoilStatusText.Foreground = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
            }
        }

        // Update trigger status
        if (TriggerStatusIndicator != null && TriggerStatusText != null)
        {
            if (_triggerActive)
            {
                TriggerStatusIndicator.Fill = FindResource("SecondaryAccentBrush") as System.Windows.Media.Brush;
                TriggerStatusText.Text = "Active";
                TriggerStatusText.Foreground = FindResource("SecondaryAccentBrush") as System.Windows.Media.Brush;
            }
            else
            {
                TriggerStatusIndicator.Fill = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
                TriggerStatusText.Text = "Inactive";
                TriggerStatusText.Foreground = FindResource("DangerAccentBrush") as System.Windows.Media.Brush;
            }
        }
    }

    private void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isPreviewRunning)
        {
            StartPreview();
        }
        else
        {
            StopPreview();
        }
    }

    private void StartPreview()
    {
        try
        {
            _previewCapture = new ScreenCapture(Configuration.Screen);
            _previewTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 30) // 30 FPS
            };
            _previewTimer.Tick += PreviewTimer_Tick;
            _previewTimer.Start();
            
            _isPreviewRunning = true;
            PreviewButton.Content = "Stop Preview";
            ShowStatusMessage("Preview started", MessageType.Success);
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to start preview: {ex.Message}", MessageType.Error);
        }
    }

    private void StopPreview()
    {
        _previewTimer?.Stop();
        _previewTimer = null;
        _previewCapture?.Dispose();
        _previewCapture = null;
        
        _isPreviewRunning = false;
        PreviewButton.Content = "Start Preview";
        PreviewImage.Source = null;
        ShowStatusMessage("Preview stopped", MessageType.Info);
    }

    private void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (_previewCapture == null) return;

            // Calculate preview region
            var screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var centerX = screenSize.Width / 2;
            var centerY = screenSize.Height / 2;
            
            var previewRegion = new System.Drawing.Rectangle(
                centerX - Configuration.Screen.FovX / 2,
                centerY - Configuration.Screen.FovY / 2,
                Configuration.Screen.FovX,
                Configuration.Screen.FovY
            );

            using var capturedImage = _previewCapture.CaptureRegion(previewRegion);
            if (capturedImage != null)
            {
                // Convert Mat to BitmapSource for display
                var bitmap = MatToBitmapSource(capturedImage);
                PreviewImage.Source = bitmap;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Preview error: {ex.Message}");
        }
    }

    private BitmapSource MatToBitmapSource(OpenCvSharp.Mat mat)
    {
        // Convert OpenCV Mat to WPF BitmapSource safely
        using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        if (bitmap == null)
        {
            // Return a transparent 1x1 bitmap or throw an exception
            return BitmapSource.Create(1, 1, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, new byte[4], 4);
        }

        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            stream.Position = 0;
            
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze for performance and thread safety
            
            return bitmapImage;
        }
    }

    private void LoadConfiguration()
    {
        try
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unibot", "config.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var savedConfig = JsonSerializer.Deserialize<Configuration>(json);
                if (savedConfig != null)
                {
                    Configuration = savedConfig;
                    ShowStatusMessage("Configuration loaded", MessageType.Success);
                }
            }
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to load configuration: {ex.Message}", MessageType.Warning);
        }
    }

    private void SaveConfiguration()
    {
        try
        {
            var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unibot");
            Directory.CreateDirectory(configDir);
            
            var configPath = Path.Combine(configDir, "config.json");
            var json = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
            
            ShowStatusMessage("Configuration saved", MessageType.Success);
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to save configuration: {ex.Message}", MessageType.Error);
        }
    }

    private void LoadAvailableComPorts()
    {
        try
        {
            AvailableComPorts = System.IO.Ports.SerialPort.GetPortNames();
            OnPropertyChanged(nameof(AvailableComPorts));
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Failed to load COM ports: {ex.Message}", MessageType.Warning);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        StopPreview();
        StopUnibot();
        SaveConfiguration();
        base.OnClosed(e);
    }

    // Add hotkey support
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        // Add global hotkey handling
        var source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        source?.AddHook(HwndHook);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        
        if (msg == WM_HOTKEY)
        {
            var key = ((int)lParam >> 16) & 0xFFFF;
            var modifier = (int)lParam & 0xFFFF;
            
            // Handle hotkeys
            if (key == 0x70) // F1 - Reload config
            {
                LoadConfiguration();
                handled = true;
            }
            else if (key == 0x73) // F4 - Exit
            {
                Close();
                handled = true;
            }
        }
        
        return IntPtr.Zero;
    }

    private enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }
}