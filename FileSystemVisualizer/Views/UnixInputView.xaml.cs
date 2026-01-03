using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class UnixInputView : Page
    {
        public UnixInputView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new UnixInputViewModel(navigationService);
        }
    }
}
