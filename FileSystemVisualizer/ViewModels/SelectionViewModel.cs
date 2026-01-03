using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Services;
using FileSystemVisualizer.Views;

namespace FileSystemVisualizer.ViewModels
{
    public class SelectionViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;

        public ICommand SelectFatCommand { get; }
        public ICommand SelectUnixCommand { get; }
        public ICommand SelectNtfsCommand { get; }

        public SelectionViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;

            SelectFatCommand = new RelayCommand(_ => NavigateToFat());
            SelectUnixCommand = new RelayCommand(_ => NavigateToUnix());
            SelectNtfsCommand = new RelayCommand(_ => NavigateToNtfs());
        }

        private void NavigateToFat()
        {
            _navigationService.NavigateTo(new FatInputFlexibleView(_navigationService));
        }

        private void NavigateToUnix()
        {
            _navigationService.NavigateTo(new UnixInputFlexibleView(_navigationService));
        }

        private void NavigateToNtfs()
        {
            _navigationService.NavigateTo(new NtfsInputFlexibleView(_navigationService));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
