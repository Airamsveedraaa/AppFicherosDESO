namespace FileSystemVisualizer.Models
{
    public class UnixConfiguration
    {
        public double DiskSizeGB { get; set; }
        public int BlockSizeKB { get; set; }
        public int PointerSizeBits { get; set; } = 32; // 32 or 64
        public int NumberOfInodes { get; set; }
        public int NumberOfFiles { get; set; }

        // Calculated properties
        public long TotalBlocks => (long)((DiskSizeGB * 1024 * 1024) / BlockSizeKB);
        public int PointersPerBlock => (BlockSizeKB * 1024) / (PointerSizeBits / 8);
        
        // Maximum file size calculation
        public long MaxFileSizeBytes
        {
            get
            {
                long blockSize = BlockSizeKB * 1024;
                long p = PointersPerBlock;
                return (12 + p + (p * p) + (p * p * p)) * blockSize;
            }
        }
    }
}
