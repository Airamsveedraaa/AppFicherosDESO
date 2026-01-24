using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace FileSystemVisualizer.Models
{
    /// <summary>
    /// Representa un archivo o directorio en el sistema FAT
    /// </summary>
    public class FatEntry : INotifyPropertyChanged
    {
        private string _name = "";
        private char _type = 'F';
        private int _startBlock = 0;
        private int _sizeInRecords = 1;
        private int _sizeInBlocks = 1;
        private int? _specificStartBlock = null;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public char Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public int StartBlock
        {
            get => _startBlock;
            set { _startBlock = value; OnPropertyChanged(); }
        }

        public int SizeInRecords
        {
            get => _sizeInRecords;
            set { _sizeInRecords = value; OnPropertyChanged(); CalculateBlocks(); }
        }

        public int SizeInBlocks
        {
            get => _sizeInBlocks;
            set { _sizeInBlocks = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Bloque inicial específico (null = asignación automática por anchura)
        /// </summary>
        public int? SpecificStartBlock
        {
            get => _specificStartBlock;
            set { _specificStartBlock = value; OnPropertyChanged(); }
        }

        private int _recordsPerBlock = 1;
        public int RecordsPerBlock 
        { 
            get => _recordsPerBlock;
            set 
            { 
                _recordsPerBlock = value; 
                OnPropertyChanged(); 
                CalculateBlocks(); 
            }
        }

        /// <summary>
        /// Etiqueta asignada automáticamente para identificación visual (A-Z)
        /// </summary>
        public char? FileLabel { get; set; }

        /// <summary>
        /// Hijos (para directorios)
        /// </summary>
        public ObservableCollection<FatEntry> Children { get; set; } = new ObservableCollection<FatEntry>();

        /// <summary>
        /// Indica si es un directorio
        /// </summary>
        public bool IsDirectory => Type == 'D';

        private void CalculateBlocks()
        {
            if (RecordsPerBlock > 0)
            {
                SizeInBlocks = (int)Math.Ceiling((double)SizeInRecords / RecordsPerBlock);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
