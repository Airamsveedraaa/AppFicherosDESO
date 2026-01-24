namespace FileSystemVisualizer.Models
{
    /// <summary>
    /// Representa un grupo de bloques consecutivos del mismo tipo para visualización optimizada
    /// </summary>
    public class BlockGroup
    {
        public int StartBlock { get; set; }
        public int EndBlock { get; set; }
        public int Count => EndBlock - StartBlock + 1;
        
        /// <summary>
        /// Tipo de bloque: "FREE", "RESERVED", "EOF", o letra del archivo (A-Z)
        /// </summary>
        public string Type { get; set; } = "";
        
        /// <summary>
        /// Nombre del archivo asociado (si aplica)
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Texto a mostrar en la columna de bloques
        /// Ejemplos: "0-1", "2-21 (...)", "74"
        /// </summary>
        public string BlockRangeDisplay
        {
            get
            {
                if (Count == 1) return StartBlock.ToString();
                if (Count >= 3) return $"{StartBlock}-{EndBlock} (...)";
                return $"{StartBlock}-{EndBlock}";
            }
        }
    }
}
