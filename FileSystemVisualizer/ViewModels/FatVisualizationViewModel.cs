using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using FileSystemVisualizer.Helpers;
using FileSystemVisualizer.Models;
using FileSystemVisualizer.Services;

namespace FileSystemVisualizer.ViewModels
{
    public class FatVisualizationViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        
        public FatConfigurationFlexible Configuration { get; }
        public FatFileSystem FileSystem { get; }

        // Directory Tree (Root is a virtual directory containing all files)
        public FatEntry RootDirectory { get; private set; }
        
        // Flat list for table display
        public ObservableCollection<FatEntry> AllFiles { get; private set; }

        // Visualization
        public ObservableCollection<BlockGroup> DiskBlocks { get; private set; }
        public ObservableCollection<FatTableEntry> FatTableEntries { get; private set; }
        public ObservableCollection<DiskRegion> DiskStructure { get; private set; }

        public string PageTitle => $"Visualización {Configuration.DeterminedFatType}";
        public string DiskSummary => $"{Configuration.DiskSize.Value?.ToString()} - Bloques: {FileSystem.TotalClusters} - RRL/BLQ: {Configuration.RecordsPerBlock}";

        public ICommand BackCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand AddDirectoryCommand { get; }
        public ICommand RemoveEntryCommand { get; }
        public ICommand RefreshCommand { get; }

        // For adding new entries
        private string _newEntryName = "";
        public string NewEntryName
        {
            get => _newEntryName;
            set { _newEntryName = value; OnPropertyChanged(); }
        }

        private int _newEntrySizeRecords = 10;
        public int NewEntrySizeRecords
        {
            get => _newEntrySizeRecords;
            set { _newEntrySizeRecords = value; OnPropertyChanged(); }
        }

        private bool _specifyStartBlock = false;
        public bool SpecifyStartBlock
        {
            get => _specifyStartBlock;
            set { _specifyStartBlock = value; OnPropertyChanged(); }
        }

        private int _newEntryStartBlock = 0;
        public int NewEntryStartBlock
        {
            get => _newEntryStartBlock;
            set { _newEntryStartBlock = value; OnPropertyChanged(); }
        }

        private char _newEntryType = 'F';
        public char NewEntryType
        {
            get => _newEntryType;
            set { _newEntryType = value; OnPropertyChanged(); }
        }

        private string _linkedFileName = "";
        public string LinkedFileName
        {
            get => _linkedFileName;
            set { _linkedFileName = value; OnPropertyChanged(); }
        }

        public FatVisualizationViewModel(NavigationService navigationService, FatConfigurationFlexible config)
        {
            _navigationService = navigationService;
            Configuration = config;
            
            // Initialize File System Logic
            FileSystem = new FatFileSystem(config);
            
            // Initialize Root Directory (virtual)
            RootDirectory = new FatEntry
            {
                Name = "Raíz",
                Type = 'D',
                SizeInRecords = 0,
                SizeInBlocks = 0,
                RecordsPerBlock = Configuration.RecordsPerBlock
            };

            // Initialize Collections
            AllFiles = new ObservableCollection<FatEntry>();
            DiskBlocks = new ObservableCollection<BlockGroup>();
            FatTableEntries = new ObservableCollection<FatTableEntry>();
            DiskStructure = new ObservableCollection<DiskRegion>(FileSystem.GetRegions());

            // Commands
            BackCommand = new RelayCommand(_ => GoBack());
            AddFileCommand = new RelayCommand(_ => AddFile());
            AddDirectoryCommand = new RelayCommand(_ => AddDirectory());
            RemoveEntryCommand = new RelayCommand(RemoveEntry);
            RefreshCommand = new RelayCommand(_ => RefreshVisualization());

            // Initial refresh
            RefreshVisualization();
        }

        private void AddFile()
        {
            if (string.IsNullOrWhiteSpace(NewEntryName)) return;

            var newFile = new FatEntry
            {
                Name = NewEntryName,
                Type = NewEntryType,
                SizeInRecords = NewEntrySizeRecords,
                RecordsPerBlock = Configuration.RecordsPerBlock,
                SpecificStartBlock = SpecifyStartBlock ? NewEntryStartBlock : null
            };

            // Hard Link: copiar info del archivo enlazado
            if (NewEntryType == 'F' && !string.IsNullOrWhiteSpace(LinkedFileName))
            {
                var linkedFile = FindFileByName(RootDirectory, LinkedFileName);
                if (linkedFile != null)
                {
                    newFile.SizeInRecords = linkedFile.SizeInRecords;
                    newFile.SizeInBlocks = linkedFile.SizeInBlocks;
                    newFile.StartBlock = linkedFile.StartBlock; // Hard link apunta al mismo bloque
                    newFile.SpecificStartBlock = linkedFile.StartBlock;
                }
            }

            // Directorios SIEMPRE ocupan 1 bloque
            if (NewEntryType == 'D')
            {
                newFile.SizeInBlocks = 1;
                newFile.SizeInRecords = Configuration.RecordsPerBlock; // Ajustar registros al bloque
            }

            RootDirectory.Children.Add(newFile);
            
            // Reset inputs
            NewEntryName = "";
            NewEntrySizeRecords = 10;
            SpecifyStartBlock = false;
            NewEntryType = 'F';
            LinkedFileName = "";
            
            RefreshVisualization();
        }

        private FatEntry? FindFileByName(FatEntry root, string name)
        {
            foreach (var child in root.Children)
            {
                if (child.Name == name) return child;
                var found = FindFileByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void AddDirectory()
        {
            if (string.IsNullOrWhiteSpace(NewEntryName)) return;

            var newDir = new FatEntry
            {
                Name = NewEntryName,
                Type = 'D',
                SizeInRecords = NewEntrySizeRecords,
                RecordsPerBlock = Configuration.RecordsPerBlock,
                SpecificStartBlock = SpecifyStartBlock ? NewEntryStartBlock : null
            };

            RootDirectory.Children.Add(newDir);
            
            // Reset inputs
            NewEntryName = "";
            NewEntrySizeRecords = 10;
            SpecifyStartBlock = false;
            
            RefreshVisualization();
        }

        private void RemoveEntry(object? parameter)
        {
            if (parameter is FatEntry entry)
            {
                RemoveFromTree(RootDirectory, entry);
                RefreshVisualization();
            }
        }

        private bool RemoveFromTree(FatEntry parent, FatEntry toRemove)
        {
            if (parent.Children.Remove(toRemove))
                return true;

            foreach (var child in parent.Children)
            {
                if (RemoveFromTree(child, toRemove))
                    return true;
            }

            return false;
        }

        private void RefreshVisualization()
        {
            // Assign blocks using breadth-first
            FileSystem.AssignBlocksBreadthFirst(RootDirectory);

            // Update FileSystem's Files list
            FileSystem.Files.Clear();
            FileSystem.Files.AddRange(FlattenTree(RootDirectory));

            // Generate FAT table
            FileSystem.GenerateFatTable();

            // Update flat list for table display
            AllFiles.Clear();
            foreach (var file in FileSystem.Files)
            {
                AllFiles.Add(file);
            }

            // Update disk blocks visualization
            DiskBlocks.Clear();
            foreach (var group in FileSystem.GetBlockGroups())
            {
                DiskBlocks.Add(group);
            }

            // Update FAT table visualization
            FatTableEntries.Clear();
            foreach (var entry in FileSystem.FatTable)
            {
                FatTableEntries.Add(entry);
            }

            OnPropertyChanged(nameof(RootDirectory));
        }

        private List<FatEntry> FlattenTree(FatEntry root)
        {
            var result = new List<FatEntry>();
            foreach (var child in root.Children)
            {
                result.Add(child);
                if (child.Children.Count > 0)
                {
                    result.AddRange(FlattenTree(child));
                }
            }
            return result;
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
