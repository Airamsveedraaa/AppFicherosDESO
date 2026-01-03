namespace FileSystemVisualizer.Models
{
    public class InodeStructure
    {
        public int DirectPointers { get; set; } = 12;
        public bool HasIndirectSimple { get; set; } = true;
        public bool HasIndirectDouble { get; set; } = true;
        public bool HasIndirectTriple { get; set; } = true;
    }

    public class UnixConfigurationFlexible
    {
        // Disk Configuration
        public DataField<UnitValue> DiskSize { get; set; } = new();
        public DataField<UnitValue> BlockSize { get; set; } = new();
        public DataField<UnitValue> SectorSize { get; set; } = new(new UnitValue(512, "Bytes"));

        // Inode Configuration
        public DataField<int> TotalInodes { get; set; } = new();
        public DataField<UnitValue> InodeSize { get; set; } = new(new UnitValue(128, "Bytes"));
        public DataField<UnitValue> InodeMetadataSize { get; set; } = new();

        // Pointer Configuration
        public int PointerSizeBytes { get; set; } = 4; // 2, 4, or 8 bytes

        // Inode Structure (configurable)
        public InodeStructure InodeStructure { get; set; } = new();

        // Block Group Configuration (optional)
        public DataField<int> NumberOfBlockGroups { get; set; } = new();
        public DataField<int> BlocksPerGroup { get; set; } = new();
        public DataField<int> InodesPerGroup { get; set; } = new();

        // Simulation
        public int NumberOfFiles { get; set; } = 5;
        public bool HierarchicalDirectories { get; set; } = false;

        // Calculated Properties
        public long TotalBlocks
        {
            get
            {
                if (!DiskSize.IsSpecified || !BlockSize.IsSpecified)
                    return 0;

                var diskBytes = DiskSize.Value!.ToBytes();
                var blockBytes = BlockSize.Value!.ToBytes();
                return diskBytes / blockBytes;
            }
        }

        public int PointersPerBlock
        {
            get
            {
                if (!BlockSize.IsSpecified)
                    return 0;

                var blockBytes = (int)BlockSize.Value!.ToBytes();
                return blockBytes / PointerSizeBytes;
            }
        }

        public long MaxFileSizeBytes
        {
            get
            {
                if (!BlockSize.IsSpecified)
                    return 0;

                long blockSize = BlockSize.Value!.ToBytes();
                long p = PointersPerBlock;

                long directSize = InodeStructure.DirectPointers * blockSize;
                long indirectSimple = InodeStructure.HasIndirectSimple ? p * blockSize : 0;
                long indirectDouble = InodeStructure.HasIndirectDouble ? p * p * blockSize : 0;
                long indirectTriple = InodeStructure.HasIndirectTriple ? p * p * p * blockSize : 0;

                return directSize + indirectSimple + indirectDouble + indirectTriple;
            }
        }
    }
}
