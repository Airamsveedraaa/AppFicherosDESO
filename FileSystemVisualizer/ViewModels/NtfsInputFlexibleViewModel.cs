using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Models;
using FileSystemVisualizer.Services;

namespace FileSystemVisualizer.ViewModels
{
    public class NtfsInputFlexibleViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        
        public NtfsConfigurationFlexible Configuration { get; }

        // Disk Configuration
        private double _diskSizeValue = 1;
        private string _diskSizeUnit = "GB";
        private double _clusterSizeValue = 4;
        private string _clusterSizeUnit = "KB";
        
        // MFT Configuration
        private double _mftEntrySizeValue = 1;
        private string _mftEntrySizeUnit = "KB";
        
        private bool _specifyMftSize = false;
        private double _mftSizeValue = 0;
        private string _mftSizeUnit = "MB";
        
        private bool _specifyMftZonePercentage = false;
        private double _mftZonePercentage = 12.5;
        
        private int _numberOfFiles = 5;
        private double _residentFilePercentage = 30.0;

        // Properties
        public double DiskSizeValue
        {
            get => _diskSizeValue;
            set { _diskSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string DiskSizeUnit
        {
            get => _diskSizeUnit;
            set { _diskSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double ClusterSizeValue
        {
            get => _clusterSizeValue;
            set { _clusterSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string ClusterSizeUnit
        {
            get => _clusterSizeUnit;
            set { _clusterSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double MftEntrySizeValue
        {
            get => _mftEntrySizeValue;
            set { _mftEntrySizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string MftEntrySizeUnit
        {
            get => _mftEntrySizeUnit;
            set { _mftEntrySizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool SpecifyMftSize
        {
            get => _specifyMftSize;
            set { _specifyMftSize = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double MftSizeValue
        {
            get => _mftSizeValue;
            set { _mftSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string MftSizeUnit
        {
            get => _mftSizeUnit;
            set { _mftSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool SpecifyMftZonePercentage
        {
            get => _specifyMftZonePercentage;
            set { _specifyMftZonePercentage = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double MftZonePercentage
        {
            get => _mftZonePercentage;
            set { _mftZonePercentage = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int NumberOfFiles
        {
            get => _numberOfFiles;
            set { _numberOfFiles = value; OnPropertyChanged(); }
        }

        public double ResidentFilePercentage
        {
            get => _residentFilePercentage;
            set { _residentFilePercentage = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand ContinueCommand { get; }

        public NtfsInputFlexibleViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            Configuration = new NtfsConfigurationFlexible();

            BackCommand = new RelayCommand(_ => GoBack());
            ContinueCommand = new RelayCommand(_ => Continue(), _ => CanContinue());

            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            Configuration.DiskSize.SetSpecifiedValue(new UnitValue(DiskSizeValue, DiskSizeUnit));
            Configuration.ClusterSize.SetSpecifiedValue(new UnitValue(ClusterSizeValue, ClusterSizeUnit));
            Configuration.MftEntrySize.SetSpecifiedValue(new UnitValue(MftEntrySizeValue, MftEntrySizeUnit));

            if (SpecifyMftSize)
            {
                Configuration.MftSize.SetSpecifiedValue(new UnitValue(MftSizeValue, MftSizeUnit));
            }
            else
            {
                Configuration.MftSize.Clear();
            }

            if (SpecifyMftZonePercentage)
            {
                Configuration.MftZonePercentage.SetSpecifiedValue(MftZonePercentage);
            }
            else
            {
                Configuration.MftZonePercentage.Clear();
            }

            Configuration.NumberOfFiles = NumberOfFiles;
            Configuration.ResidentFilePercentage = ResidentFilePercentage;
        }

        private bool CanContinue()
        {
            return DiskSizeValue > 0 && ClusterSizeValue > 0 && MftEntrySizeValue > 0 && NumberOfFiles > 0;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void Continue()
        {
            var mftSizeGB = Configuration.CalculatedMftSizeBytes / (1024.0 * 1024.0 * 1024.0);
            var mftSizeDisplay = SpecifyMftSize 
                ? $"{MftSizeValue} {MftSizeUnit} (especificado)" 
                : $"{mftSizeGB:N2} GB (calculado - {Configuration.MftZonePercentage.GetValueOrDefault()}% del disco)";

            System.Windows.MessageBox.Show(
                $"Configuración NTFS:\n\n" +
                $"Tamaño del disco: {DiskSizeValue} {DiskSizeUnit}\n" +
                $"Tamaño del clúster: {ClusterSizeValue} {ClusterSizeUnit}\n" +
                $"Clústeres totales: {Configuration.TotalClusters:N0}\n\n" +
                $"Tamaño de entrada MFT: {MftEntrySizeValue} {MftEntrySizeUnit}\n" +
                $"Tamaño de la MFT: {mftSizeDisplay}\n" +
                $"Archivos máximos: {Configuration.MaxFiles:N0}\n\n" +
                $"Número de archivos: {NumberOfFiles}\n" +
                $"Archivos residentes: {ResidentFilePercentage}%",
                "Configuración NTFS Flexible",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
