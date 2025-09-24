using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PGCTimeTracker_V2.Services;
using PGCTimeTracker_V2.ViewModels;
using System;

namespace PGCTimeTracker_V2;

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
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Children =
                        {
                            new TextBlock { Text = "System is idle. Click OK to continue." },
                            new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
                        }
                }
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