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
        public DataField<int> NumberOfFatCopies { get; set; } = new(2);
        public DataField<UnitValue> FatSize { get; set; } = new();

        // Root Directory (FAT12/16 only)
        public DataField<int> RootDirectoryEntries { get; set; } = new(512);
        public DataField<int> DirectoryEntrySize { get; set; } = new(32);

        // Simulation
        public int NumberOfFiles { get; set; } = 5;

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

            int bytesPerEntry = fatType switch
            {
                "FAT12" => 2, // Aproximado (1.5 bytes redondeado)
                "FAT16" => 2,
                "FAT32" => 4,
                _ => 2
            };

            return blocks * bytesPerEntry;
        }
    }
}
