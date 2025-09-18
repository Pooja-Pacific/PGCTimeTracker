using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PGCTimeTracker.ViewModels;

namespace PGCTimeTracker;

public partial class IdleWindow : Window
{
    public IdleWindow(string username)
    {
        InitializeComponent();
        DataContext = new IdleViewModel(username);
    }
}