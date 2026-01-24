using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Models;
using FileSystemVisualizer.Services;

namespace FileSystemVisualizer.ViewModels
{
    public class UnixInputFlexibleViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        
        public UnixConfigurationFlexible Configuration { get; }

        // Disk Configuration
        private double _diskSizeValue = 1;
        private string _diskSizeUnit = "GB";
        private double _blockSizeValue = 4;
        private string _blockSizeUnit = "KB";
        
        // Optional fields
        private bool _specifyInodeCount = false;
        private int _inodeCount = 10000;
        
        private bool _specifyInodeSize = true;
        private double _inodeSizeValue = 128;
        private string _inodeSizeUnit = "Bytes";
        
        // Inode structure
        private int _directPointers = 12;
        private bool _hasIndirectSimple = true;
        private bool _hasIndirectDouble = true;
        private bool _hasIndirectTriple = true;
        
        private int _pointerSizeBytes = 4;
        private int _numberOfFiles = 5;
        
        // Geometry Configuration
        // (Mode selector removed)

        // Geometry Params
        private int _cylinders = 1000;
        private int _heads = 16;
        private int _sectorsPerTrack = 63;
        private int _geometrySectorSize = 512;
        
        // Total Counts
        // (Backing fields moved logic moved below)

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

        public double BlockSizeValue
        {
            get => _blockSizeValue;
            set { _blockSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string BlockSizeUnit
        {
            get => _blockSizeUnit;
            set { _blockSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool SpecifyInodeCount
        {
            get => _specifyInodeCount;
            set { _specifyInodeCount = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int InodeCount
        {
            get => _inodeCount;
            set { _inodeCount = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool SpecifyInodeSize
        {
            get => _specifyInodeSize;
            set { _specifyInodeSize = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double InodeSizeValue
        {
            get => _inodeSizeValue;
            set { _inodeSizeValue = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public string InodeSizeUnit
        {
            get => _inodeSizeUnit;
            set { _inodeSizeUnit = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int DirectPointers
        {
            get => _directPointers;
            set { _directPointers = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool HasIndirectSimple
        {
            get => _hasIndirectSimple;
            set { _hasIndirectSimple = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool HasIndirectDouble
        {
            get => _hasIndirectDouble;
            set { _hasIndirectDouble = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public bool HasIndirectTriple
        {
            get => _hasIndirectTriple;
            set { _hasIndirectTriple = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int PointerSizeBytes
        {
            get => _pointerSizeBytes;
            set { _pointerSizeBytes = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int NumberOfFiles
        {
            get => _numberOfFiles;
            set { _numberOfFiles = value; OnPropertyChanged(); }
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

        public UnixInputFlexibleViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            Configuration = new UnixConfigurationFlexible();

            BackCommand = new RelayCommand(_ => GoBack());
            ContinueCommand = new RelayCommand(_ => Continue(), _ => CanContinue());

            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            Configuration.DiskSize.SetSpecifiedValue(new UnitValue(DiskSizeValue, DiskSizeUnit));
            Configuration.BlockSize.SetSpecifiedValue(new UnitValue(BlockSizeValue, BlockSizeUnit));

            if (SpecifyInodeCount)
            {
                Configuration.TotalInodes.SetSpecifiedValue(InodeCount);
            }
            else
            {
                Configuration.TotalInodes.Clear();
            }

            if (SpecifyInodeSize)
            {
                Configuration.InodeSize.SetSpecifiedValue(new UnitValue(InodeSizeValue, InodeSizeUnit));
            }
            else
            {
                Configuration.InodeSize.Clear();
            }

            Configuration.PointerSizeBytes = PointerSizeBytes;
            Configuration.InodeStructure.DirectPointers = DirectPointers;
            Configuration.InodeStructure.HasIndirectSimple = HasIndirectSimple;
            Configuration.InodeStructure.HasIndirectDouble = HasIndirectDouble;
            Configuration.InodeStructure.HasIndirectTriple = HasIndirectTriple;
            Configuration.InodeStructure.HasIndirectTriple = HasIndirectTriple;
            Configuration.NumberOfFiles = NumberOfFiles;
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

                    long blockSize = (long)new UnitValue(BlockSizeValue, BlockSizeUnit).ToBytes();
                    if (blockSize > 0)
                    {
                        _totalBlocksInput = totalBytes / blockSize;
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

                    long blockSize = (long)new UnitValue(BlockSizeValue, BlockSizeUnit).ToBytes();
                    if (blockSize > 0)
                    {
                        _totalBlocksInput = totalBytes / blockSize;
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
                    long blockSize = (long)new UnitValue(BlockSizeValue, BlockSizeUnit).ToBytes();
                    long totalBytes = TotalBlocksInput * blockSize;
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

                    long blockSize = (long)new UnitValue(BlockSizeValue, BlockSizeUnit).ToBytes();
                    if (blockSize > 0)
                    {
                        _totalBlocksInput = totalBytes / blockSize;
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
            return DiskSizeValue > 0 && BlockSizeValue > 0 && NumberOfFiles > 0 && DirectPointers > 0;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void Continue()
        {
            var maxFileSize = Configuration.MaxFileSizeBytes;
            var maxFileSizeMB = maxFileSize / (1024.0 * 1024.0);

            System.Windows.MessageBox.Show(
                $"Configuración Unix/EXT:\n\n" +
                $"Tamaño del disco: {DiskSizeValue} {DiskSizeUnit}\n" +
                $"Tamaño del bloque: {BlockSizeValue} {BlockSizeUnit}\n" +
                $"Bloques totales: {Configuration.TotalBlocks:N0}\n\n" +
                $"I-nodos: {(SpecifyInodeCount ? $"{InodeCount:N0} (especificado)" : "No especificado")}\n" +
                $"Tamaño del i-nodo: {(SpecifyInodeSize ? $"{InodeSizeValue} {InodeSizeUnit} (especificado)" : "128 Bytes (por defecto)")}\n\n" +
                $"Estructura del i-nodo:\n" +
                $"  • Punteros directos: {DirectPointers}\n" +
                $"  • Indirecto simple: {(HasIndirectSimple ? "Sí" : "No")}\n" +
                $"  • Indirecto doble: {(HasIndirectDouble ? "Sí" : "No")}\n" +
                $"  • Indirecto triple: {(HasIndirectTriple ? "Sí" : "No")}\n" +
                $"  • Tamaño del puntero: {PointerSizeBytes} bytes\n\n" +
                $"Punteros por bloque: {Configuration.PointersPerBlock}\n" +
                $"Tamaño máximo de archivo: {maxFileSizeMB:N2} MB\n\n" +
                $"Número de archivos: {NumberOfFiles}",
                "Configuración Unix/EXT Flexible",
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
