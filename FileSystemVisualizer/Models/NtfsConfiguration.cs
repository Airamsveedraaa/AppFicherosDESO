namespace FileSystemVisualizer.Models
{
    public class NtfsConfiguration
    {
        public double DiskSizeGB { get; set; }
        public int ClusterSizeKB { get; set; }
        public int MftEntrySizeKB { get; set; } = 1; // Typically 1 KB
        public int NumberOfFiles { get; set; }

        // Calculated properties
        public long TotalClusters => (long)((DiskSizeGB * 1024 * 1024) / ClusterSizeKB);
        public long MftSizeGB => (long)(DiskSizeGB * 0.125); // 12.5% of disk
        public long MaxFiles => (MftSizeGB * 1024 * 1024) / MftEntrySizeKB;
    }
}
