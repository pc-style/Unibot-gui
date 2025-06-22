using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Unibot.Models;

namespace Unibot.Core;

public class UnibotEngine : IDisposable
{
    private readonly Configuration _config;
    private readonly ScreenCapture _screenCapture;
    private readonly MouseControl _mouseControl;
    
    private bool _isRunning = false;
    private bool _disposed = false;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _mainLoopTask;

    // Aim state
    private double _previousX = 0;
    private double _previousY = 0;
    private double _moveX = 0;
    private double _moveY = 0;

    // Recoil state
    private double _recoilOffset = 0;

    // Timing
    private DateTime _lastFrameTime = DateTime.Now;

    // Key state tracking
    private readonly Dictionary<int, bool> _keyStates = new();

    // Windows API imports for key checking
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public event EventHandler<bool>? AimStateChanged;
    public event EventHandler<bool>? TriggerStateChanged;
    public event EventHandler<bool>? RecoilStateChanged;
    public event EventHandler<bool>? RapidFireStateChanged;

    public UnibotEngine(Configuration config)
    {
        _config = config;
        _screenCapture = new ScreenCapture(_config.Screen);
        _mouseControl = new MouseControl(_config.Communication, _config.RapidFire.TargetCps);
    }

    public bool IsRunning => _isRunning;

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _mainLoopTask = Task.Run(() => MainLoop(_cancellationTokenSource.Token));
        
        System.Diagnostics.Debug.WriteLine("Unibot engine started");
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        _mainLoopTask?.Wait(5000); // Wait up to 5 seconds for graceful shutdown
        
        System.Diagnostics.Debug.WriteLine("Unibot engine stopped");
    }

    private async Task MainLoop(CancellationToken cancellationToken)
    {
        var targetFrameTime = TimeSpan.FromSeconds(1.0 / _config.Screen.Fps);
        
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            var frameStart = DateTime.Now;
            var deltaTime = (frameStart - _lastFrameTime).TotalSeconds;
            _lastFrameTime = frameStart;

            try
            {
                // Check for config reload key
                if (CheckKeyPressed(_config.KeyBinds.KeyReloadConfig))
                {
                    // TODO: Implement config reload
                    System.Diagnostics.Debug.WriteLine("Config reload requested");
                }

                // Check for exit key
                if (CheckKeyPressed(_config.KeyBinds.KeyExit))
                {
                    Stop();
                    break;
                }

                // Get current key states
                var aimState = GetAimState();
                var triggerState = GetTriggerState();
                var recoilState = GetRecoilState();
                var rapidFireState = GetRapidFireState();

                // Process frame if needed
                if (aimState || triggerState || (_config.Debug.Enabled && _config.Debug.AlwaysOn))
                {
                    await ProcessFrame(deltaTime, aimState, triggerState, recoilState, rapidFireState);
                }

                // Handle rapid fire
                if (rapidFireState)
                {
                    _mouseControl.Click();
                }

                // Apply recoil compensation
                ApplyRecoilCompensation(recoilState, deltaTime);

                // Apply calculated mouse movement
                _mouseControl.Move(_moveX, _moveY);
                
                // Reset movement for next frame
                _moveX = 0;
                _moveY = 0;

                // Frame rate limiting
                var frameTime = DateTime.Now - frameStart;
                if (frameTime < targetFrameTime)
                {
                    await Task.Delay(targetFrameTime - frameTime, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in main loop: {ex.Message}");
                await Task.Delay(100, cancellationToken); // Brief pause before retrying
            }
        }
    }

    private async Task ProcessFrame(double deltaTime, bool aimState, bool triggerState, bool recoilState, bool rapidFireState)
    {
        // Calculate screen regions
        var screenSize = GetScreenSize();
        var screenCenter = new Point(screenSize.Width / 2, screenSize.Height / 2);
        
        var fovRegion = new Rectangle(
            screenCenter.X - _config.Screen.FovX / 2,
            screenCenter.Y - _config.Screen.FovY / 2 - _config.Aim.Offset,
            _config.Screen.FovX,
            _config.Screen.FovY
        );

        // Detect targets
        var (target, trigger) = _screenCapture.DetectTarget(fovRegion, (int)_recoilOffset);

        // Handle triggerbot
        if (triggerState && trigger)
        {
            var delay = _config.Trigger.TriggerDelay > 0 
                ? (_config.Trigger.TriggerDelay + Random.Shared.Next(_config.Trigger.TriggerRandomization)) / 1000.0
                : 0;
            
            _mouseControl.Click(delay);
        }

        // Handle aim assist
        if (aimState && target.HasValue)
        {
            CalculateAim(target.Value);
        }
    }

    private void CalculateAim(Point target)
    {
        // Apply speed multipliers
        var x = target.X * _config.Aim.Speed;
        var y = target.Y * _config.Aim.Speed * _config.Aim.YSpeed;

        // Apply smoothing
        x = (1 - _config.Aim.Smooth) * _previousX + _config.Aim.Smooth * x;
        y = (1 - _config.Aim.Smooth) * _previousY + _config.Aim.Smooth * y;

        // Store for next frame
        _previousX = x;
        _previousY = y;

        // Set movement values
        _moveX = x;
        _moveY = y;
    }

    private void ApplyRecoilCompensation(bool recoilState, double deltaTime)
    {
        if (!recoilState || deltaTime <= 0) 
        {
            _recoilOffset = 0;
            return;
        }

        var isMouseDown = (GetAsyncKeyState(0x01) & 0x8000) != 0; // Left mouse button

        switch (_config.Recoil.Mode)
        {
            case RecoilMode.Move when isMouseDown:
                _moveX += _config.Recoil.RecoilX * deltaTime;
                _moveY += _config.Recoil.RecoilY * deltaTime;
                break;

            case RecoilMode.Offset:
                if (isMouseDown)
                {
                    if (_recoilOffset < _config.Recoil.MaxOffset)
                    {
                        _recoilOffset += _config.Recoil.RecoilY * deltaTime;
                        if (_recoilOffset > _config.Recoil.MaxOffset)
                            _recoilOffset = _config.Recoil.MaxOffset;
                    }
                }
                else
                {
                    if (_recoilOffset > 0)
                    {
                        _recoilOffset -= _config.Recoil.Recover * deltaTime;
                        if (_recoilOffset < 0)
                            _recoilOffset = 0;
                    }
                }
                break;
        }
    }

    private bool GetAimState()
    {
        return _config.KeyBinds.AimKeys.Any(key => (GetAsyncKeyState(key) & 0x8000) != 0);
    }

    private bool GetTriggerState()
    {
        return (GetAsyncKeyState(_config.KeyBinds.KeyTrigger) & 0x8000) != 0;
    }

    private bool GetRecoilState()
    {
        return (GetAsyncKeyState(_config.KeyBinds.KeyToggleRecoil) & 0x8000) != 0;
    }

    private bool GetRapidFireState()
    {
        return (GetAsyncKeyState(_config.KeyBinds.KeyRapidFire) & 0x8000) != 0;
    }

    private bool CheckKeyPressed(int virtualKeyCode)
    {
        var isPressed = (GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;
        var wasPressed = _keyStates.GetValueOrDefault(virtualKeyCode, false);
        
        _keyStates[virtualKeyCode] = isPressed;
        
        // Return true only on key down edge (not held)
        return isPressed && !wasPressed;
    }

    private Size GetScreenSize()
    {
        if (_config.Screen.AutoDetectResolution)
        {
            return new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }
        
        return new Size(_config.Screen.ResolutionX, _config.Screen.ResolutionY);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Stop();
            _cancellationTokenSource?.Dispose();
            _screenCapture?.Dispose();
            _mouseControl?.Dispose();
            _disposed = true;
        }
    }
}