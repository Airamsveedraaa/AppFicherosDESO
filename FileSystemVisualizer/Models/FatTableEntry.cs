namespace FileSystemVisualizer.Models
{
    /// <summary>
    /// Representa una entrada en la tabla FAT
    /// </summary>
    public class FatTableEntry
    {
        public int BlockIndex { get; set; }
        
        /// <summary>
        /// Siguiente bloque en la cadena. 
        /// null = FREE, -1 = EOF, -2 = RESERVED, número positivo = siguiente bloque
        /// </summary>
        public int? NextBlock { get; set; }
        
        /// <summary>
        /// Etiqueta del archivo (A-Z) para identificación visual
        /// </summary>
        public char? FileLabel { get; set; }

        /// <summary>
        /// Texto a mostrar en la parte superior del bloque
        /// Ejemplos: "EOF", "FREE", "X", "4", "A"
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (NextBlock == null) return "FREE";
                if (NextBlock == -1) return "EOF";
                if (NextBlock == -2) return "X"; // Reserved
                return NextBlock.ToString()!;
            }
        }

        /// <summary>
        /// Indica si este bloque está reservado para el sistema (Boot, FAT, Root)
        /// </summary>
        public bool IsReserved { get; set; }
    }
}
