using Avalonia.Controls;
using Avalonia.Threading;
using PGCTimeTracker.ViewModels;

namespace PGCTimeTracker.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();

    }
}
