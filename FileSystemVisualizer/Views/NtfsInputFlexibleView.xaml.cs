using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class NtfsInputFlexibleView : Page
    {
        public NtfsInputFlexibleView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new NtfsInputFlexibleViewModel(navigationService);
        }
    }
}
