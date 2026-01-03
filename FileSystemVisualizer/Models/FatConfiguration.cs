namespace FileSystemVisualizer.Models
{
    public class FatConfiguration
    {
        public double DiskSizeGB { get; set; }
        public int ClusterSizeKB { get; set; }
        public string FatType { get; set; } = "Auto"; // "FAT12", "FAT16", "FAT32", "Auto"
        public int NumberOfFiles { get; set; }

        // Calculated properties
        public long TotalBlocks => (long)((DiskSizeGB * 1024 * 1024) / ClusterSizeKB);
        public string DeterminedFatType
        {
            get
            {
                if (FatType != "Auto") return FatType;
                
                if (TotalBlocks <= 4096) return "FAT12";
                if (TotalBlocks <= 65536) return "FAT16";
                return "FAT32";
            }
        }
    }
}
