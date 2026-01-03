using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class SelectionView : Page
    {
        public SelectionView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new SelectionViewModel(navigationService);
        }
    }
}
