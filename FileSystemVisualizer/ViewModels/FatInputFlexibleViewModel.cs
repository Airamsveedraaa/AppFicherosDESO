using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Models;
using FileSystemVisualizer.Services;

namespace FileSystemVisualizer.ViewModels
{
    public class FatInputFlexibleViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        
        public FatConfigurationFlexible Configuration { get; }

        // Disk Configuration
        private double _diskSizeValue = 1;
        private string _diskSizeUnit = "GB";
        private double _clusterSizeValue = 4;
        private string _clusterSizeUnit = "KB";
        
        // Optional fields
        private bool _specifySectorSize = false;
        private double _sectorSizeValue = 512;
        private string _sectorSizeUnit = "Bytes";
        
        private bool _specifyFatSize = false;
        private double _fatSizeValue = 0;
        private string _fatSizeUnit = "KB";
        
        private string _fatType = "Auto";
        private int _numberOfFiles = 5;

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

        public bool SpecifySectorSize
        {
            get => _specifySectorSize;
            set { _specifySectorSize = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double SectorSizeValue
        {
            get => _sectorSizeValue;
            set { _sectorSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string SectorSizeUnit
        {
            get => _sectorSizeUnit;
            set { _sectorSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool SpecifyFatSize
        {
            get => _specifyFatSize;
            set { _specifyFatSize = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double FatSizeValue
        {
            get => _fatSizeValue;
            set { _fatSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string FatSizeUnit
        {
            get => _fatSizeUnit;
            set { _fatSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string FatType
        {
            get => _fatType;
            set { _fatType = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int NumberOfFiles
        {
            get => _numberOfFiles;
            set { _numberOfFiles = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand ContinueCommand { get; }

        public FatInputFlexibleViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            Configuration = new FatConfigurationFlexible();

            BackCommand = new RelayCommand(_ => GoBack());
            ContinueCommand = new RelayCommand(_ => Continue(), _ => CanContinue());

            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            // Update disk size
            Configuration.DiskSize.SetSpecifiedValue(new UnitValue(DiskSizeValue, DiskSizeUnit));

            // Update cluster size
            Configuration.ClusterSize.SetSpecifiedValue(new UnitValue(ClusterSizeValue, ClusterSizeUnit));

            // Update sector size (optional)
            if (SpecifySectorSize)
            {
                Configuration.SectorSize.SetSpecifiedValue(new UnitValue(SectorSizeValue, SectorSizeUnit));
            }
            else
            {
                Configuration.SectorSize.Clear();
            }

            // Update FAT size (optional)
            if (SpecifyFatSize)
            {
                Configuration.FatSize.SetSpecifiedValue(new UnitValue(FatSizeValue, FatSizeUnit));
            }
            else
            {
                // Calculate FAT size
                var calculatedSize = Configuration.CalculateFatSize();
                Configuration.FatSize.SetCalculatedValue(new UnitValue(calculatedSize, "Bytes"));
            }

            Configuration.FatType = FatType;
            Configuration.NumberOfFiles = NumberOfFiles;
        }

        private bool CanContinue()
        {
            return DiskSizeValue > 0 && ClusterSizeValue > 0 && NumberOfFiles > 0;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void Continue()
        {
            var fatSizeDisplay = Configuration.FatSize.IsCalculated 
                ? $"{Configuration.FatSize.Value!.ToKB():N2} KB (calculado)" 
                : $"{Configuration.FatSize.Value!.ToKB():N2} KB (especificado)";

            System.Windows.MessageBox.Show(
                $"Configuración FAT:\n\n" +
                $"Tamaño del disco: {DiskSizeValue} {DiskSizeUnit}\n" +
                $"Tamaño del clúster: {ClusterSizeValue} {ClusterSizeUnit}\n" +
                $"Tamaño del sector: {(SpecifySectorSize ? $"{SectorSizeValue} {SectorSizeUnit} (especificado)" : "512 Bytes (por defecto)")}\n\n" +
                $"Tipo FAT: {Configuration.DeterminedFatType}\n" +
                $"Bloques totales: {Configuration.TotalBlocks:N0}\n" +
                $"Tamaño de FAT: {fatSizeDisplay}\n\n" +
                $"Número de archivos: {NumberOfFiles}",
                "Configuración FAT Flexible",
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
