using Avalonia.Controls;
using PGCTimeTracker.ViewModels;

namespace PGCTimeTracker.Views
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