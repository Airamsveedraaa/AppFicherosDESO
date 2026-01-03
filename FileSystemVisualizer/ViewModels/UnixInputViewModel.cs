using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Models;
using FileSystemVisualizer.Services;

namespace FileSystemVisualizer.ViewModels
{
    public class UnixInputViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private double _diskSizeGB = 1;
        private int _blockSizeKB = 4;
        private int _pointerSizeBits = 32;
        private int _numberOfInodes = 1000;
        private int _numberOfFiles = 5;

        public UnixConfiguration Configuration { get; private set; }

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

        public int BlockSizeKB
        {
            get => _blockSizeKB;
            set
            {
                _blockSizeKB = value;
                OnPropertyChanged();
                UpdateConfiguration();
            }
        }

        public int PointerSizeBits
        {
            get => _pointerSizeBits;
            set
            {
                _pointerSizeBits = value;
                OnPropertyChanged();
                UpdateConfiguration();
            }
        }

        public int NumberOfInodes
        {
            get => _numberOfInodes;
            set
            {
                _numberOfInodes = value;
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

        public UnixInputViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            Configuration = new UnixConfiguration();

            BackCommand = new RelayCommand(_ => GoBack());
            ContinueCommand = new RelayCommand(_ => Continue(), _ => CanContinue());

            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            Configuration.DiskSizeGB = DiskSizeGB;
            Configuration.BlockSizeKB = BlockSizeKB;
            Configuration.PointerSizeBits = PointerSizeBits;
            Configuration.NumberOfInodes = NumberOfInodes;
            Configuration.NumberOfFiles = NumberOfFiles;
        }

        private bool CanContinue()
        {
            return DiskSizeGB > 0 && BlockSizeKB > 0 && NumberOfInodes > 0 && NumberOfFiles > 0;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void Continue()
        {
            // TODO: Navigate to visualization view
            System.Windows.MessageBox.Show(
                $"Configuración Unix/EXT:\n" +
                $"Tamaño del disco: {DiskSizeGB} GB\n" +
                $"Tamaño del bloque: {BlockSizeKB} KB\n" +
                $"Tamaño del puntero: {PointerSizeBits} bits\n" +
                $"Bloques totales: {Configuration.TotalBlocks}\n" +
                $"Punteros por bloque: {Configuration.PointersPerBlock}\n" +
                $"Número de i-nodos: {NumberOfInodes}\n" +
                $"Número de archivos: {NumberOfFiles}",
                "Configuración Unix/EXT");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
