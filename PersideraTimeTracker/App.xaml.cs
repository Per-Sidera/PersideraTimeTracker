using System.Windows;
using PersideraTimeTracker.Models;
using PersideraTimeTracker.Services;
using PersideraTimeTracker.ViewModels;
using PersideraTimeTracker.Views;

namespace PersideraTimeTracker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = AppSettings.Load();
        var mainVm = new MainViewModel(settings);
        var main = new MainWindow { DataContext = mainVm };

        // First-run Mercury setup wizard if not yet configured.
        string? token = null;
        try { token = CredentialManager.GetToken(); } catch { /* ignore */ }

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(settings.MercuryCustomerId))
        {
            var setup = new MercurySetupWindow(settings);
            setup.ShowDialog();
            settings.Save();
        }

        main.Show();
    }
}
