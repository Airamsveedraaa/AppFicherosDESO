using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class FatInputView : Page
    {
        public FatInputView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new FatInputViewModel(navigationService);
        }
    }
}
