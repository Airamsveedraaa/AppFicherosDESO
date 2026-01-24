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
        
        // Block Size (alias for Cluster for UI clarity)
        private double _blockSizeValue = 4;
        private string _blockSizeUnit = "KB";
        private bool _autoCalculateBlockSize = true;
        private double _blockSizeMultiplier = 1.0; // Block = N × Sector
        
        private bool _specifyFatSize = false;
        private double _fatSizeValue = 0;
        private string _fatSizeUnit = "KB";
        
        private string _fatType = "Auto";
        private int _numberOfFiles = 5;

        // Geometry Configuration
        // Geometry Configuration
        // Guard to prevent infinite loops during bidirectional updates
        // Guard to prevent infinite loops during bidirectional updates
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

        // Geometry Params (Restored)
        private int _cylinders = 1000;
        private int _heads = 16;
        private int _sectorsPerTrack = 63;
        
        // FAT Specifics
        private int _reservedSectors = 1;
        private int _numberOfFats = 2;



        public int ReservedBlocks
        {
            get => _reservedSectors;
            set { _reservedSectors = value; Configuration.ReservedBlocks.Value = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public int NumberOfFats
        {
            get => _numberOfFats;
            set { _numberOfFats = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        // Record-based sizing
        private int _recordSizeBytes = 128;
        public int RecordSizeBytes
        {
            get => _recordSizeBytes;
            set
            {
                _recordSizeBytes = value;
                Configuration.RecordSizeBytes = value; // Sync to Configuration
                OnPropertyChanged();
                OnPropertyChanged(nameof(RecordsPerBlock));
                OnPropertyChanged(nameof(FatSizeBlocks));
                UpdateConfiguration();
            }
        }

        public int RecordsPerBlock
        {
            get
            {
                if (ClusterSizeValue <= 0 || RecordSizeBytes <= 0) return 1;
                var clusterBytes = (int)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                return clusterBytes / RecordSizeBytes;
            }
        }

        public int FatSizeBlocks
        {
            get
            {
                try
                {
                    // Esto se calculará después de UpdateConfiguration
                    return Configuration?.CalculateFatSizeInBlocks() ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public string FatPointerSize
        {
            get
            {
                try
                {
                    if (Configuration == null) return "2 bytes";
                    var fatType = Configuration.DeterminedFatType;
                    return fatType switch
                    {
                        "FAT12" => "1.5 bytes (12 bits)",
                        "FAT16" => "2 bytes (16 bits)",
                        "FAT32" => "4 bytes (32 bits)",
                        _ => "2 bytes"
                    };
                }
                catch
                {
                    return "Error";
                }
            }
        }

        public long FatSizeBytes
        {
            get
            {
                try
                {
                    return Configuration?.CalculateFatSize() ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public string FatFormula
        {
            get
            {
                try
                {
                    if (Configuration == null) return "Calculando...";
                    var totalBlocks = Configuration.TotalBlocks;
                    var fatType = Configuration.DeterminedFatType;
                    var bitsPerEntry = fatType switch
                    {
                        "FAT12" => 12,
                        "FAT16" => 16,
                        "FAT32" => 32,
                        _ => 16
                    };
                    return $"{totalBlocks} bloques × {bitsPerEntry} bits / 8 = {FatSizeBytes} bytes";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

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
                    // Update configuration always calling last
                    if (!_isRecalculating) UpdateConfiguration(); 
                }
            }
        }

        public string DiskSizeUnit
        {
            get => _diskSizeUnit;
            set 
            { 
                if (_diskSizeUnit != value)
                {
                    _diskSizeUnit = value; 
                    OnPropertyChanged(); 
                    if (!_isRecalculating) UpdateFromCapacity();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        public double ClusterSizeValue
        {
            get => _clusterSizeValue;
            set 
            { 
                if (_clusterSizeValue != value)
                {
                    _clusterSizeValue = value; 
                    OnPropertyChanged();
                    if (!_isRecalculating) UpdateFromClusterSize();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        public string ClusterSizeUnit
        {
            get => _clusterSizeUnit;
            set 
            { 
                if (_clusterSizeUnit != value)
                {
                    _clusterSizeUnit = value; 
                    OnPropertyChanged();
                    if (!_isRecalculating) UpdateFromClusterSize();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        public bool SpecifySectorSize
        {
            get => _specifySectorSize;
            set { _specifySectorSize = value; OnPropertyChanged(); UpdateConfiguration(); }
        }

        public double SectorSizeValue
        {
            get => _sectorSizeValue;
            set 
            { 
                if (_sectorSizeValue != value)
                {
                    _sectorSizeValue = value; 
                    OnPropertyChanged();
                    if (!_isRecalculating) UpdateFromSectorSize();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        public string SectorSizeUnit
        {
            get => _sectorSizeUnit;
            set 
            { 
                if (_sectorSizeUnit != value)
                {
                    _sectorSizeUnit = value; 
                    OnPropertyChanged();
                    if (!_isRecalculating) UpdateFromSectorSize();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        // Block Size Properties
        public double BlockSizeValue
        {
            get => _blockSizeValue;
            set 
            { 
                if (_blockSizeValue != value)
                {
                    _blockSizeValue = value; 
                    OnPropertyChanged();
                    
                    // Sync with ClusterSize for compatibility
                    _clusterSizeValue = value;
                    OnPropertyChanged(nameof(ClusterSizeValue));
                    
                    if (!_isRecalculating) UpdateFromBlockSize();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        public string BlockSizeUnit
        {
            get => _blockSizeUnit;
            set 
            { 
                if (_blockSizeUnit != value)
                {
                    _blockSizeUnit = value; 
                    OnPropertyChanged();
                    
                    // Sync with ClusterSizeUnit
                    _clusterSizeUnit = value;
                    OnPropertyChanged(nameof(ClusterSizeUnit));
                    
                    if (!_isRecalculating) UpdateFromBlockSize();
                    if (!_isRecalculating) UpdateConfiguration();
                }
            }
        }

        public bool AutoCalculateBlockSize
        {
            get => _autoCalculateBlockSize;
            set 
            { 
                if (_autoCalculateBlockSize != value)
                {
                    _autoCalculateBlockSize = value; 
                    OnPropertyChanged();
                    
                    // Si se activa auto-cálculo, recalcular ahora
                    if (value && !_isRecalculating)
                    {
                        UpdateFromSectorSize();
                    }
                }
            }
        }

        public double BlockSizeMultiplier
        {
            get => _blockSizeMultiplier;
            set 
            { 
                if (_blockSizeMultiplier != value)
                {
                    _blockSizeMultiplier = value; 
                    OnPropertyChanged();
                    
                    // Si auto-cálculo está activo, recalcular
                    if (AutoCalculateBlockSize && !_isRecalculating)
                    {
                        UpdateFromSectorSize();
                    }
                }
            }
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

        public int Cylinders
        {
            get => _cylinders;
            set 
            {
                 _cylinders = value; 
                 OnPropertyChanged(); 
                 if (!_isRecalculating) UpdateFromGeometry();
            }
        }

        public int Heads
        {
            get => _heads;
            set 
            { 
                _heads = value; 
                OnPropertyChanged(); 
                if (!_isRecalculating) UpdateFromGeometry();
            }
        }

        public int SectorsPerTrack
        {
            get => _sectorsPerTrack;
            set 
            { 
                _sectorsPerTrack = value; 
                OnPropertyChanged(); 
                if (!_isRecalculating) UpdateFromGeometry();
            }
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

            Configuration.ReservedBlocks.SetSpecifiedValue(ReservedBlocks);
            Configuration.NumberOfFatCopies.SetSpecifiedValue(NumberOfFats);
            Configuration.FatType = FatType;
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
                    // 1. Calculate Total Sectors
                    _totalSectorsInput = (long)Cylinders * Heads * SectorsPerTrack;
                    OnPropertyChanged(nameof(TotalSectorsInput));

                    // 2. Calculate Capacity
                    long sectorSize = (long)new UnitValue(SpecifySectorSize ? SectorSizeValue : 512, SectorSizeUnit).ToBytes();
                    long totalBytes = _totalSectorsInput * sectorSize;
                    UpdateCapacityFields(totalBytes);

                    // 3. Calculate Total Blocks (Clusters)
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

        private void UpdateFromTotalSectors()
        {
            _isRecalculating = true;
            try
            {
                if (TotalSectorsInput > 0)
                {
                    // 1. Calculate Capacity
                    long sectorSize = (long)new UnitValue(SpecifySectorSize ? SectorSizeValue : 512, SectorSizeUnit).ToBytes();
                    long totalBytes = TotalSectorsInput * sectorSize;
                    UpdateCapacityFields(totalBytes);

                    // 2. Calculate Total Blocks
                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    if (clusterSize > 0)
                    {
                        _totalBlocksInput = totalBytes / clusterSize;
                        OnPropertyChanged(nameof(TotalBlocksInput));
                    }
                    
                    // NOTE: We do NOT reverse-engineer Geometry (C/H/S) as it's ambiguous.
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
                    // 1. Calculate Capacity
                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    long totalBytes = TotalBlocksInput * clusterSize;
                    UpdateCapacityFields(totalBytes);

                    // 2. Calculate Total Sectors
                    long sectorSize = (long)new UnitValue(SpecifySectorSize ? SectorSizeValue : 512, SectorSizeUnit).ToBytes();
                    if (sectorSize > 0)
                    {
                        _totalSectorsInput = totalBytes / sectorSize;
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
                    // 1. Calculate Total Sectors
                    long sectorSize = (long)new UnitValue(SpecifySectorSize ? SectorSizeValue : 512, SectorSizeUnit).ToBytes();
                    if (sectorSize > 0)
                    {
                        _totalSectorsInput = totalBytes / sectorSize;
                        OnPropertyChanged(nameof(TotalSectorsInput));
                    }

                    // 2. Calculate Total Blocks
                    long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                    if (clusterSize > 0)
                    {
                        _totalBlocksInput = totalBytes / clusterSize;
                        OnPropertyChanged(nameof(TotalBlocksInput));
                    }
                }
                UpdateConfiguration(); // Ensure model has the capacity set
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

        private void UpdateFromClusterSize()
        {
            _isRecalculating = true;
            try
            {
                long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                long diskSize = (long)new UnitValue(DiskSizeValue, DiskSizeUnit).ToBytes();

                if (clusterSize > 0 && diskSize > 0)
                {
                    _totalBlocksInput = diskSize / clusterSize;
                    OnPropertyChanged(nameof(TotalBlocksInput));
                }
            }
            finally { _isRecalculating = false; }
        }

        private void UpdateFromSectorSize()
        {
            _isRecalculating = true;
            try
            {
                long sectorSize = (long)new UnitValue(SectorSizeValue, SectorSizeUnit).ToBytes();
                
                // Auto-calcular tamaño de bloque si está habilitado
                if (AutoCalculateBlockSize && BlockSizeMultiplier > 0)
                {
                    long blockBytes = (long)(sectorSize * BlockSizeMultiplier);
                    _blockSizeValue = ConvertBytesToUnit(blockBytes, BlockSizeUnit);
                    OnPropertyChanged(nameof(BlockSizeValue));
                    
                    // Sync con ClusterSize
                    _clusterSizeValue = _blockSizeValue;
                    _clusterSizeUnit = BlockSizeUnit;
                    OnPropertyChanged(nameof(ClusterSizeValue));
                    OnPropertyChanged(nameof(ClusterSizeUnit));
                }
                
                // Si tenemos geometría, recalcular capacidad total
                if (Cylinders > 0 && Heads > 0 && SectorsPerTrack > 0)
                {
                    _totalSectorsInput = (long)Cylinders * Heads * SectorsPerTrack;
                    OnPropertyChanged(nameof(TotalSectorsInput));
                    
                    long totalBytes = _totalSectorsInput * sectorSize;
                    UpdateCapacityFields(totalBytes);
                }
                // Si tenemos total de sectores, recalcular capacidad
                else if (TotalSectorsInput > 0)
                {
                    long totalBytes = TotalSectorsInput * sectorSize;
                    UpdateCapacityFields(totalBytes);
                }
                
                // Recalcular total de bloques
                long clusterSize = (long)new UnitValue(ClusterSizeValue, ClusterSizeUnit).ToBytes();
                long diskSize = (long)new UnitValue(DiskSizeValue, DiskSizeUnit).ToBytes();
                if (clusterSize > 0 && diskSize > 0)
                {
                    _totalBlocksInput = diskSize / clusterSize;
                    OnPropertyChanged(nameof(TotalBlocksInput));
                }
            }
            finally { _isRecalculating = false; }
        }

        private void UpdateFromBlockSize()
        {
            _isRecalculating = true;
            try
            {
                long blockSize = (long)new UnitValue(BlockSizeValue, BlockSizeUnit).ToBytes();
                long diskSize = (long)new UnitValue(DiskSizeValue, DiskSizeUnit).ToBytes();
                
                // Recalcular total de bloques
                if (blockSize > 0 && diskSize > 0)
                {
                    _totalBlocksInput = diskSize / blockSize;
                    OnPropertyChanged(nameof(TotalBlocksInput));
                }
            }
            finally { _isRecalculating = false; }
        }

        private double ConvertBytesToUnit(long bytes, string unit)
        {
            return unit switch
            {
                "Bytes" => bytes,
                "KB" => bytes / 1024.0,
                "MB" => bytes / (1024.0 * 1024.0),
                "GB" => bytes / (1024.0 * 1024.0 * 1024.0),
                _ => bytes
            };
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
            var visualizationPage = new Views.FatVisualizationView(new FatVisualizationViewModel(_navigationService, Configuration));
            _navigationService.NavigateTo(visualizationPage);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
