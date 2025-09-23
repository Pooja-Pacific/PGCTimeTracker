using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PGCTimeTracker.Services;
using PGCTimeTracker.ViewModels;
using System;

namespace PGCTimeTracker;

public partial class IdleWindow : Window
{
    private readonly IdleTrackerService _idleTrackerService;
    public IdleWindow(string username)
    {
        InitializeComponent();
        DataContext = new IdleViewModel(username);

        _idleTrackerService = new IdleTrackerService(TimeSpan.FromMinutes(5)); // idle = 5 min
        _idleTrackerService.OnIdle += HandleIdle;
        _idleTrackerService.OnResume += HandleResume;
        // Listen for user input globally
        this.PointerMoved += (_, __) => _idleTrackerService.UpdateActivity();
        this.KeyDown += (_, __) => _idleTrackerService.UpdateActivity();
    }
    private void HandleIdle()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var dlg = new Window
            {
                Title = "Idle",
                Content = new TextBlock { Text = "System is idle. Click OK to continue." },
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (this.IsVisible && this.WindowState != WindowState)
            {
                dlg.ShowDialog(this);
            }
            else
            {
                dlg.Show();
            }
        });
    }

    private void HandleResume()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Console.WriteLine("User resumed from idle.");
        });
    }
}