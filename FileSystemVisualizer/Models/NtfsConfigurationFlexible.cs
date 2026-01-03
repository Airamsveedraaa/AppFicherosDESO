namespace FileSystemVisualizer.Models
{
    public class NtfsConfigurationFlexible
    {
        // Disk Configuration
        public DataField<UnitValue> DiskSize { get; set; } = new();
        public DataField<UnitValue> ClusterSize { get; set; } = new();
        public DataField<UnitValue> SectorSize { get; set; } = new(new UnitValue(512, "Bytes"));

        // MFT Configuration
        public DataField<UnitValue> MftEntrySize { get; set; } = new(new UnitValue(1, "KB"));
        public DataField<double> MftZonePercentage { get; set; } = new(12.5);
        public DataField<UnitValue> MftSize { get; set; } = new();

        // Entry Attributes (optional)
        public DataField<UnitValue> EntryHeaderSize { get; set; } = new();
        public DataField<UnitValue> AvailableAttributeSpace { get; set; } = new();
        public DataField<UnitValue> AverageResidentAttributeSize { get; set; } = new();

        // Simulation
        public int NumberOfFiles { get; set; } = 5;
        public double ResidentFilePercentage { get; set; } = 30.0; // % of files that are resident

        // Calculated Properties
        public long TotalClusters
        {
            get
            {
                if (!DiskSize.IsSpecified || !ClusterSize.IsSpecified)
                    return 0;

                var diskBytes = DiskSize.Value!.ToBytes();
                var clusterBytes = ClusterSize.Value!.ToBytes();
                return diskBytes / clusterBytes;
            }
        }

        public long CalculatedMftSizeBytes
        {
            get
            {
                if (MftSize.IsSpecified)
                    return MftSize.Value!.ToBytes();

                if (!DiskSize.IsSpecified)
                    return 0;

                var diskBytes = DiskSize.Value!.ToBytes();
                var percentage = MftZonePercentage.GetValueOrDefault() / 100.0;
                return (long)(diskBytes * percentage);
            }
        }

        public long MaxFiles
        {
            get
            {
                if (!MftEntrySize.IsSpecified)
                    return 0;

                var mftSize = CalculatedMftSizeBytes;
                var entrySize = MftEntrySize.Value!.ToBytes();
                return mftSize / entrySize;
            }
        }
    }
}
