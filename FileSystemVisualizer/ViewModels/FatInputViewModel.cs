using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Models;
using FileSystemVisualizer.Services;

namespace FileSystemVisualizer.ViewModels
{
    public class FatInputViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private double _diskSizeGB = 1;
        private int _clusterSizeKB = 4;
        private string _fatType = "Auto";
        private int _numberOfFiles = 5;

        public FatConfiguration Configuration { get; private set; }

        public double DiskSizeGB
        {
            get => _diskSizeGB;
            set
            {
                _diskSizeGB = value;
                OnPropertyChanged();
                UpdateConfiguration();
            }
        }

        public int ClusterSizeKB
        {
            get => _clusterSizeKB;
            set
            {
                _clusterSizeKB = value;
                OnPropertyChanged();
                UpdateConfiguration();
            }
        }

        public string FatType
        {
            get => _fatType;
            set
            {
                _fatType = value;
                OnPropertyChanged();
                UpdateConfiguration();
            }
        }

        public int NumberOfFiles
        {
            get => _numberOfFiles;
            set
            {
                _numberOfFiles = value;
                OnPropertyChanged();
                UpdateConfiguration();
            }
        }

        public ICommand BackCommand { get; }
        public ICommand ContinueCommand { get; }

        public FatInputViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            Configuration = new FatConfiguration();

            BackCommand = new RelayCommand(_ => GoBack());
            ContinueCommand = new RelayCommand(_ => Continue(), _ => CanContinue());

            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            Configuration.DiskSizeGB = DiskSizeGB;
            Configuration.ClusterSizeKB = ClusterSizeKB;
            Configuration.FatType = FatType;
            Configuration.NumberOfFiles = NumberOfFiles;
        }

        private bool CanContinue()
        {
            return DiskSizeGB > 0 && ClusterSizeKB > 0 && NumberOfFiles > 0;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void Continue()
        {
            // TODO: Navigate to visualization view
            System.Windows.MessageBox.Show(
                $"Configuración FAT:\n" +
                $"Tamaño del disco: {DiskSizeGB} GB\n" +
                $"Tamaño del clúster: {ClusterSizeKB} KB\n" +
                $"Tipo FAT: {Configuration.DeterminedFatType}\n" +
                $"Bloques totales: {Configuration.TotalBlocks}\n" +
                $"Número de archivos: {NumberOfFiles}",
                "Configuración FAT");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
