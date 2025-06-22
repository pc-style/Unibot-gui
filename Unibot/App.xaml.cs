using System.Windows;
using ModernWpf;

namespace Unibot;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Set dark theme
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        ThemeManager.Current.AccentColor = System.Windows.Media.Color.FromRgb(0, 120, 212);
        
        base.OnStartup(e);
    }
}