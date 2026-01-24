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

        // Geometry Configuration
        // (Mode selector removed)

        // Geometry Params
        private int _cylinders = 1000;
        private int _heads = 16;
        private int _sectorsPerTrack = 63;
        private int _geometrySectorSize = 512;
        
        // Total Counts
        // (Backing fields moved below guard logic)

        // Guard logic
        private bool _isRecalculating = false;

        private long _totalSectorsInput = 20480;
        private long _totalBlocksInput = 5000;

        public long TotalSectorsInput
        {
            get => _totalSectorsInput;
            set 
            {
                 if (_totalSectorsInput != value)
                 {
                     _totalSectorsInput = value; 
                     OnPropertyChanged();
                     if (!_isRecalculating) UpdateFromTotalSectors();
                 }
            }
        }

        public long TotalBlocksInput
        {
            get => _totalBlocksInput;
            set 
            {
                 if (_totalBlocksInput != value)
                 {
                     _totalBlocksInput = value; 
                     OnPropertyChanged(); 
                     if (!_isRecalculating) UpdateFromTotalBlocks();
                 }
            }
        }



        // Properties
        public double DiskSizeValue
        {
            get => _diskSizeValue;
            set 
            { 
                if (_diskSizeValue != value)
                {
                    _diskSizeValue = value; 
                    OnPropertyChanged(); 
                    if (!_isRecalculating) UpdateFromCapacity();
                    if (!_isRecalculating) UpdateConfiguration(); 
                }
            }
        }

        public string DiskSizeUnit
        {
            get => _diskSizeUnit;
            set 
            {
                 _diskSizeUnit = value; 
                 OnPropertyChanged(); 
                 if (!_isRecalculating) UpdateFromCapacity();
                 if (!_isRecalculating) UpdateConfiguration(); 
            }
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

        public int Cylinders
        {
            get => _cylinders;
            set { _cylinders = value; OnPropertyChanged(); if (!_isRecalculating) UpdateFromGeometry(); }
        }

        public int Heads
        {
            get => _heads;
            set { _heads = value; OnPropertyChanged(); if (!_isRecalculating) UpdateFromGeometry(); }
        }

        public int SectorsPerTrack
        {
            get => _sectorsPerTrack;
            set { _sectorsPerTrack = value; OnPropertyChanged(); if (!_isRecalculating) UpdateFromGeometry(); }
        }

        public int GeometrySectorSize
        {
            get => _geometrySectorSize;
            set { _geometrySectorSize = value; OnPropertyChanged(); if (!_isRecalculating) UpdateFromGeometry(); }
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

        // --- Unified Update Logic ---

        private void UpdateFromGeometry()
        {
            _isRecalculating = true;
            try
            {
                if (Cylinders > 0 && Heads > 0 && SectorsPerTrack > 0)
                {
                    _totalSectorsInput = (long)Cylinders * Heads * SectorsPerTrack;
                    OnPropertyChanged(nameof(TotalSectorsInput));

                    long sectorBytes = GeometrySectorSize > 0 ? GeometrySectorSize : 512;
                    long totalBytes = _totalSectorsInput * sectorBytes;
                    UpdateCapacityFields(totalBytes);

                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    if (clusterSize > 0)
                    {
                        _totalBlocksInput = totalBytes / clusterSize;
                        OnPropertyChanged(nameof(TotalBlocksInput));
                    }
                }
                UpdateConfiguration(); // Sync model
            }
            finally { _isRecalculating = false; }
        }

        private void UpdateFromTotalSectors()
        {
            _isRecalculating = true;
            try
            {
                if (TotalSectorsInput > 0)
                {
                    long sectorBytes = GeometrySectorSize > 0 ? GeometrySectorSize : 512;
                    long totalBytes = TotalSectorsInput * sectorBytes;
                    UpdateCapacityFields(totalBytes);

                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    if (clusterSize > 0)
                    {
                        _totalBlocksInput = totalBytes / clusterSize;
                        OnPropertyChanged(nameof(TotalBlocksInput));
                    }
                }
                UpdateConfiguration();
            }
            finally { _isRecalculating = false; }
        }

        private void UpdateFromTotalBlocks()
        {
            _isRecalculating = true;
            try
            {
                if (TotalBlocksInput > 0)
                {
                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    long totalBytes = TotalBlocksInput * clusterSize;
                    UpdateCapacityFields(totalBytes);

                    long sectorBytes = GeometrySectorSize > 0 ? GeometrySectorSize : 512;
                    if (sectorBytes > 0)
                    {
                        _totalSectorsInput = totalBytes / sectorBytes;
                        OnPropertyChanged(nameof(TotalSectorsInput));
                    }
                }
                UpdateConfiguration();
            }
            finally { _isRecalculating = false; }
        }

        private void UpdateFromCapacity()
        {
            _isRecalculating = true;
            try
            {
                long totalBytes = (long)new UnitValue(DiskSizeValue, DiskSizeUnit).ToBytes();
                if (totalBytes > 0)
                {
                     long sectorBytes = GeometrySectorSize > 0 ? GeometrySectorSize : 512;
                    if (sectorBytes > 0)
                    {
                        _totalSectorsInput = totalBytes / sectorBytes;
                        OnPropertyChanged(nameof(TotalSectorsInput));
                    }

                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    if (clusterSize > 0)
                    {
                        _totalBlocksInput = totalBytes / clusterSize;
                        OnPropertyChanged(nameof(TotalBlocksInput));
                    }
                }
                UpdateConfiguration();
            }
            finally { _isRecalculating = false; }
        }

        private void UpdateCapacityFields(long totalBytes)
        {
             if (totalBytes >= 1024 * 1024 * 1024)
            {
                _diskSizeValue = totalBytes / (1024.0 * 1024.0 * 1024.0);
                _diskSizeUnit = "GB";
            }
            else if (totalBytes >= 1024 * 1024)
            {
                _diskSizeValue = totalBytes / (1024.0 * 1024.0);
                _diskSizeUnit = "MB";
            }
            else
            {
                _diskSizeValue = totalBytes / 1024.0;
                _diskSizeUnit = "KB";
            }
            OnPropertyChanged(nameof(DiskSizeValue));
            OnPropertyChanged(nameof(DiskSizeUnit));
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
