using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class FatInputFlexibleView : Page
    {
        public FatInputFlexibleView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new FatInputFlexibleViewModel(navigationService);
        }
    }
}
