using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class UnixInputFlexibleView : Page
    {
        public UnixInputFlexibleView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new UnixInputFlexibleViewModel(navigationService);
        }
    }
}
