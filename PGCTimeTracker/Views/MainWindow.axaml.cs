using Avalonia.Controls;
using PGCTimeTracker_V2.ViewModels;

namespace PGCTimeTracker_V2.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}