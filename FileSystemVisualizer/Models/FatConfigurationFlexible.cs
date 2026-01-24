namespace FileSystemVisualizer.Models
{
    public class FatConfigurationFlexible
    {
        // Disk Configuration
        public DataField<UnitValue> DiskSize { get; set; } = new();
        public DataField<UnitValue> SectorSize { get; set; } = new(new UnitValue(512, "Bytes"));
        public DataField<UnitValue> ClusterSize { get; set; } = new();
        public DataField<int> SectorsPerCluster { get; set; } = new();

        // FAT Configuration
        public string FatType { get; set; } = "Auto"; // "Auto", "FAT12", "FAT16", "FAT32"
        public DataField<int> ReservedBlocks { get; set; } = new(3); // Boot + reserved area in blocks
        public DataField<int> NumberOfFatCopies { get; set; } = new(2);
        public DataField<UnitValue> FatSize { get; set; } = new();

        // Root Directory (FAT12/16 only)
        public DataField<int> RootDirectoryEntries { get; set; } = new(512);
        public DataField<int> DirectoryEntrySize { get; set; } = new(32);

        // Simulation
        public int NumberOfFiles { get; set; } = 5;

        // Record-based sizing (for exercises)
        public int RecordSizeBytes { get; set; } = 128;
        public int RecordsPerBlock
        {
            get
            {
                if (!ClusterSize.IsSpecified || RecordSizeBytes == 0)
                    return 1;
                var blockBytes = (int)ClusterSize.Value!.ToBytes(SectorSize.GetValueOrDefault()?.ToBytes() is long sb ? (int)sb : null);
                return blockBytes / RecordSizeBytes;
            }
        }

        // Calculated Properties
        public long TotalBlocks
        {
            get
            {
                if (!DiskSize.IsSpecified || !ClusterSize.IsSpecified)
                    return 0;

                var diskBytes = DiskSize.Value!.ToBytes();
                var clusterBytes = ClusterSize.Value!.ToBytes(SectorSize.GetValueOrDefault()?.ToBytes() is long sb ? (int)sb : null);
                return diskBytes / clusterBytes;
            }
        }

        public string DeterminedFatType
        {
            get
            {
                if (FatType != "Auto") return FatType;

                var blocks = TotalBlocks;
                if (blocks <= 4096) return "FAT12";
                if (blocks <= 65536) return "FAT16";
                return "FAT32";
            }
        }

        public long CalculateFatSize()
        {
            var blocks = TotalBlocks;
            var fatType = DeterminedFatType;

            int bitsPerEntry = fatType switch
            {
                "FAT12" => 12,
                "FAT16" => 16,
                "FAT32" => 32,
                _ => 16
            };

            // Fórmula del PDF: (Nº bloques × bits por entrada) / 8
            long fatSizeBytes = (blocks * bitsPerEntry) / 8;
            return fatSizeBytes;
        }

        public int CalculateFatSizeInBlocks()
        {
            if (!ClusterSize.IsSpecified)
                return 0;

            var fatSizeBytes = CalculateFatSize();
            var blockBytes = (long)ClusterSize.Value!.ToBytes(SectorSize.GetValueOrDefault()?.ToBytes() is long sb ? (int)sb : null);
            
            // Redondear hacia arriba
            return (int)Math.Ceiling((double)fatSizeBytes / blockBytes);
        }
    }
}
