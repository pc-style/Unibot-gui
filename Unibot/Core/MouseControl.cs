using System.Drawing;
using System.IO.Ports;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Unibot.Models;

namespace Unibot.Core;

public class MouseControl : IDisposable
{
    private readonly CommunicationSettings _settings;
    private SerialPort? _serialPort;
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private bool _disposed = false;
    private readonly object _lockObject = new();
    private DateTime _lastClickTime = DateTime.MinValue;
    private readonly int _targetCps;

    // Mouse movement remainder for sub-pixel accuracy
    private double _remainderX = 0;
    private double _remainderY = 0;

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    public MouseControl(CommunicationSettings settings, int targetCps)
    {
        _settings = settings;
        _targetCps = targetCps;
        InitializeCommunication();
    }

    private void InitializeCommunication()
    {
        switch (_settings.Type)
        {
            case CommunicationType.Serial:
                InitializeSerial();
                break;
            
            case CommunicationType.Socket:
                InitializeSocket();
                break;
            
            case CommunicationType.Driver:
                // TODO: Initialize interception driver
                break;
            
            case CommunicationType.None:
            default:
                // Uses Windows API directly
                break;
        }
    }

    private void InitializeSerial()
    {
        try
        {
            _serialPort = new SerialPort(_settings.ComPort, 115200);
            _serialPort.Open();
            System.Diagnostics.Debug.WriteLine($"Serial port {_settings.ComPort} opened successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open serial port: {ex.Message}");
            _serialPort?.Dispose();
            _serialPort = null;
        }
    }

    private void InitializeSocket()
    {
        try
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_settings.Ip, _settings.Port);
            _networkStream = _tcpClient.GetStream();
            System.Diagnostics.Debug.WriteLine($"Socket connection to {_settings.Ip}:{_settings.Port} established");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to connect to socket: {ex.Message}");
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
            _networkStream = null;
            _tcpClient = null;
        }
    }

    public void Move(double deltaX, double deltaY)
    {
        if (deltaX == 0 && deltaY == 0) return;

        // Add previous remainder for sub-pixel accuracy
        deltaX += _remainderX;
        deltaY += _remainderY;

        // Convert to integers and calculate new remainder
        int moveX = (int)deltaX;
        int moveY = (int)deltaY;
        _remainderX = deltaX - moveX;
        _remainderY = deltaY - moveY;

        if (moveX == 0 && moveY == 0) return;

        switch (_settings.Type)
        {
            case CommunicationType.Serial:
                SendSerialCommand($"M{moveX},{moveY}\r");
                break;
            
            case CommunicationType.Socket:
                SendSocketCommand($"M{moveX},{moveY}\r");
                break;
            
            case CommunicationType.Driver:
                // TODO: Use interception driver
                System.Diagnostics.Debug.WriteLine($"[Driver] Move({moveX}, {moveY})");
                break;
            
            case CommunicationType.None:
            default:
                mouse_event(MOUSEEVENTF_MOVE, (uint)moveX, (uint)moveY, 0, UIntPtr.Zero);
                System.Diagnostics.Debug.WriteLine($"[WinAPI] Move({moveX}, {moveY})");
                break;
        }
    }

    public void Click(double delayBeforeClick = 0)
    {
        // Rate limiting
        var timeSinceLastClick = DateTime.Now - _lastClickTime;
        var minInterval = TimeSpan.FromSeconds(1.0 / _targetCps);
        
        if (timeSinceLastClick < minInterval)
        {
            return;
        }

        _lastClickTime = DateTime.Now;

        // Execute click in a separate task to avoid blocking
        Task.Run(async () =>
        {
            if (delayBeforeClick > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayBeforeClick));
            }

            await PerformClick();
        });
    }

    private async Task PerformClick()
    {
        lock (_lockObject)
        {
            switch (_settings.Type)
            {
                case CommunicationType.Serial:
                    SendSerialCommand("C\r");
                    break;
                
                case CommunicationType.Socket:
                    SendSocketCommand("C\r");
                    break;
                
                case CommunicationType.Driver:
                    // TODO: Use interception driver with random delay
                    System.Diagnostics.Debug.WriteLine("[Driver] Click()");
                    break;
                
                case CommunicationType.None:
                default:
                    PerformWinApiClick();
                    break;
            }
        }

        // Small delay after click to prevent spam
        await Task.Delay(Random.Shared.Next(25, 35));
    }

    private void PerformWinApiClick()
    {
        var randomDelay = Random.Shared.Next(40, 80);
        
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        Thread.Sleep(randomDelay);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        
        System.Diagnostics.Debug.WriteLine($"[WinAPI] Click({randomDelay}ms)");
    }

    private void SendSerialCommand(string command)
    {
        try
        {
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Write(command);
                System.Diagnostics.Debug.WriteLine($"[Serial] Sent: {command.Trim()}");
                
                // Wait for response
                var response = _serialPort.ReadLine();
                System.Diagnostics.Debug.WriteLine($"[Serial] Response: {response}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Serial communication error: {ex.Message}");
        }
    }

    private void SendSocketCommand(string command)
    {
        try
        {
            if (_networkStream != null)
            {
                var data = Encoding.UTF8.GetBytes(command);
                _networkStream.Write(data, 0, data.Length);
                System.Diagnostics.Debug.WriteLine($"[Socket] Sent: {command.Trim()}");
                
                // Wait for response
                var buffer = new byte[4];
                var bytesRead = _networkStream.Read(buffer, 0, buffer.Length);
                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                System.Diagnostics.Debug.WriteLine($"[Socket] Response: {response}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Socket communication error: {ex.Message}");
        }
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
            _serialPort?.Dispose();
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
            _disposed = true;
        }
    }
}