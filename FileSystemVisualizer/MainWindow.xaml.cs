using System.Windows;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.Views;

namespace FileSystemVisualizer
{
    public partial class MainWindow : Window
    {
        private readonly NavigationService _navigationService;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize navigation service
            _navigationService = new NavigationService();
            _navigationService.SetFrame(MainFrame);

            // Navigate to selection view
            _navigationService.NavigateTo(new SelectionView(_navigationService));
        }
    }
}