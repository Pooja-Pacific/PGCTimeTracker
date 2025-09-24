using Avalonia.Controls;
using Avalonia.Threading;
using PGCTimeTracker_V2.ViewModels;

namespace PGCTimeTracker_V2.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();

    }
}
