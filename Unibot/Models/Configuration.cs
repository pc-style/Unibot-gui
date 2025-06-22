using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Unibot.Models;

public class Configuration : INotifyPropertyChanged
{
    public CommunicationSettings Communication { get; set; } = new();
    public ScreenSettings Screen { get; set; } = new();
    public AimSettings Aim { get; set; } = new();
    public RecoilSettings Recoil { get; set; } = new();
    public TriggerSettings Trigger { get; set; } = new();
    public RapidFireSettings RapidFire { get; set; } = new();
    public KeyBindSettings KeyBinds { get; set; } = new();
    public DebugSettings Debug { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class CommunicationSettings : INotifyPropertyChanged
{
    private CommunicationType _type = CommunicationType.None;
    private string _ip = "0.0.0.0";
    private int _port = 50124;
    private string _comPort = "COM1";

    public CommunicationType Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    public string Ip
    {
        get => _ip;
        set { _ip = value; OnPropertyChanged(); }
    }

    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    public string ComPort
    {
        get => _comPort;
        set { _comPort = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ScreenSettings : INotifyPropertyChanged
{
    private int _detectionThresholdX = 3;
    private int _detectionThresholdY = 3;
    private ColorRange _colorRange = new() { Lower = new(58, 210, 80), Upper = new(63, 255, 255) };
    private int _fovX = 256;
    private int _fovY = 256;
    private int _aimFovX = 256;
    private int _aimFovY = 256;
    private int _fps = 60;
    private bool _autoDetectResolution = true;
    private int _resolutionX = 1920;
    private int _resolutionY = 1080;
    private int _triggerThreshold = 8;
    private double _aimHeight = 0.5;

    public int DetectionThresholdX
    {
        get => _detectionThresholdX;
        set { _detectionThresholdX = value; OnPropertyChanged(); }
    }

    public int DetectionThresholdY
    {
        get => _detectionThresholdY;
        set { _detectionThresholdY = value; OnPropertyChanged(); }
    }

    public ColorRange ColorRange
    {
        get => _colorRange;
        set { _colorRange = value; OnPropertyChanged(); }
    }

    public int FovX
    {
        get => _fovX;
        set { _fovX = value; OnPropertyChanged(); }
    }

    public int FovY
    {
        get => _fovY;
        set { _fovY = value; OnPropertyChanged(); }
    }

    public int AimFovX
    {
        get => _aimFovX;
        set { _aimFovX = value; OnPropertyChanged(); }
    }

    public int AimFovY
    {
        get => _aimFovY;
        set { _aimFovY = value; OnPropertyChanged(); }
    }

    public int Fps
    {
        get => _fps;
        set { _fps = value; OnPropertyChanged(); }
    }

    public bool AutoDetectResolution
    {
        get => _autoDetectResolution;
        set { _autoDetectResolution = value; OnPropertyChanged(); }
    }

    public int ResolutionX
    {
        get => _resolutionX;
        set { _resolutionX = value; OnPropertyChanged(); }
    }

    public int ResolutionY
    {
        get => _resolutionY;
        set { _resolutionY = value; OnPropertyChanged(); }
    }

    public int TriggerThreshold
    {
        get => _triggerThreshold;
        set { _triggerThreshold = value; OnPropertyChanged(); }
    }

    public double AimHeight
    {
        get => _aimHeight;
        set { _aimHeight = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class AimSettings : INotifyPropertyChanged
{
    private int _offset = 0;
    private double _smooth = 0.0;
    private double _speed = 1.0;
    private double _ySpeed = 1.0;
    private double _aimHeight = 0.5;

    public int Offset
    {
        get => _offset;
        set { _offset = value; OnPropertyChanged(); }
    }

    public double Smooth
    {
        get => _smooth;
        set { _smooth = value; OnPropertyChanged(); }
    }

    public double Speed
    {
        get => _speed;
        set { _speed = value; OnPropertyChanged(); }
    }

    public double YSpeed
    {
        get => _ySpeed;
        set { _ySpeed = value; OnPropertyChanged(); }
    }

    public double AimHeight
    {
        get => _aimHeight;
        set { _aimHeight = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RecoilSettings : INotifyPropertyChanged
{
    private RecoilMode _mode = RecoilMode.Move;
    private double _recoilX = 0.0;
    private double _recoilY = 0.0;
    private int _maxOffset = 100;
    private double _recover = 0.0;

    public RecoilMode Mode
    {
        get => _mode;
        set { _mode = value; OnPropertyChanged(); }
    }

    public double RecoilX
    {
        get => _recoilX;
        set { _recoilX = value; OnPropertyChanged(); }
    }

    public double RecoilY
    {
        get => _recoilY;
        set { _recoilY = value; OnPropertyChanged(); }
    }

    public int MaxOffset
    {
        get => _maxOffset;
        set { _maxOffset = value; OnPropertyChanged(); }
    }

    public double Recover
    {
        get => _recover;
        set { _recover = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TriggerSettings : INotifyPropertyChanged
{
    private int _triggerDelay = 0;
    private int _triggerRandomization = 30;
    private int _triggerThreshold = 8;

    public int TriggerDelay
    {
        get => _triggerDelay;
        set { _triggerDelay = value; OnPropertyChanged(); }
    }

    public int TriggerRandomization
    {
        get => _triggerRandomization;
        set { _triggerRandomization = value; OnPropertyChanged(); }
    }

    public int TriggerThreshold
    {
        get => _triggerThreshold;
        set { _triggerThreshold = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RapidFireSettings : INotifyPropertyChanged
{
    private int _targetCps = 10;

    public int TargetCps
    {
        get => _targetCps;
        set { _targetCps = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class KeyBindSettings : INotifyPropertyChanged
{
    private int _keyReloadConfig = 0x70; // F1
    private int _keyToggleAim = 0x71; // F2
    private int _keyToggleRecoil = 0x72; // F3
    private int _keyExit = 0x73; // F4
    private int _keyTrigger = 0x06; // Mouse 4
    private int _keyRapidFire = 0x05; // Mouse 5
    private int[] _aimKeys = { 0x01, 0x02 }; // Mouse1 & Mouse2

    public int KeyReloadConfig
    {
        get => _keyReloadConfig;
        set { _keyReloadConfig = value; OnPropertyChanged(); }
    }

    public int KeyToggleAim
    {
        get => _keyToggleAim;
        set { _keyToggleAim = value; OnPropertyChanged(); }
    }

    public int KeyToggleRecoil
    {
        get => _keyToggleRecoil;
        set { _keyToggleRecoil = value; OnPropertyChanged(); }
    }

    public int KeyExit
    {
        get => _keyExit;
        set { _keyExit = value; OnPropertyChanged(); }
    }

    public int KeyTrigger
    {
        get => _keyTrigger;
        set { _keyTrigger = value; OnPropertyChanged(); }
    }

    public int KeyRapidFire
    {
        get => _keyRapidFire;
        set { _keyRapidFire = value; OnPropertyChanged(); }
    }

    public int[] AimKeys
    {
        get => _aimKeys;
        set { _aimKeys = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DebugSettings : INotifyPropertyChanged
{
    private bool _enabled = true;
    private bool _alwaysOn = true;
    private DisplayMode _displayMode = DisplayMode.Mask;

    public bool Enabled
    {
        get => _enabled;
        set { _enabled = value; OnPropertyChanged(); }
    }

    public bool AlwaysOn
    {
        get => _alwaysOn;
        set { _alwaysOn = value; OnPropertyChanged(); }
    }

    public DisplayMode DisplayMode
    {
        get => _displayMode;
        set { _displayMode = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ColorRange : INotifyPropertyChanged
{
    private HsvColor _lower = new();
    private HsvColor _upper = new();

    public HsvColor Lower
    {
        get => _lower;
        set { _lower = value; OnPropertyChanged(); }
    }

    public HsvColor Upper
    {
        get => _upper;
        set { _upper = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class HsvColor : INotifyPropertyChanged
{
    private int _h;
    private int _s;
    private int _v;

    public HsvColor() { }
    public HsvColor(int h, int s, int v)
    {
        H = h;
        S = s;
        V = v;
    }

    public int H
    {
        get => _h;
        set { _h = value; OnPropertyChanged(); }
    }

    public int S
    {
        get => _s;
        set { _s = value; OnPropertyChanged(); }
    }

    public int V
    {
        get => _v;
        set { _v = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum CommunicationType
{
    None,
    Driver,
    Serial,
    Socket
}

public enum RecoilMode
{
    Move,
    Offset
}

public enum DisplayMode
{
    Game,
    Mask
}