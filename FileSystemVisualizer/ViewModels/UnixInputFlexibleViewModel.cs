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
            Configuration.NumberOfFiles = NumberOfFiles;
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
