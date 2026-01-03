using System.Windows.Controls;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class NtfsInputView : Page
    {
        public NtfsInputView(NavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new NtfsInputViewModel(navigationService);
        }
    }
}
