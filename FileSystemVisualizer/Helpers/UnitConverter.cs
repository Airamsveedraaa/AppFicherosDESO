using System;

namespace FileSystemVisualizer.Helpers
{
    public static class UnitConverter
    {
        // Standard conversions
        public const int BYTES_PER_KB = 1024;
        public const int BYTES_PER_MB = 1024 * 1024;
        public const long BYTES_PER_GB = 1024L * 1024L * 1024L;
        public const int DEFAULT_SECTOR_SIZE = 512; // bytes

        // Convert any value to bytes
        public static long ToBytes(double value, string unit, int? sectorSize = null, int? blockSize = null)
        {
            return unit.ToUpper() switch
            {
                "BYTES" or "B" => (long)value,
                "KB" => (long)(value * BYTES_PER_KB),
                "MB" => (long)(value * BYTES_PER_MB),
                "GB" => (long)(value * BYTES_PER_GB),
                "SECTORES" or "SECTORS" => (long)(value * (sectorSize ?? DEFAULT_SECTOR_SIZE)),
                "BLOQUES" or "BLOCKS" => blockSize.HasValue ? (long)(value * blockSize.Value) : throw new InvalidOperationException("Block size required for block conversion"),
                _ => throw new ArgumentException($"Unknown unit: {unit}")
            };
        }

        // Convert bytes to specific unit
        public static double FromBytes(long bytes, string targetUnit, int? sectorSize = null, int? blockSize = null)
        {
            return targetUnit.ToUpper() switch
            {
                "BYTES" or "B" => bytes,
                "KB" => bytes / (double)BYTES_PER_KB,
                "MB" => bytes / (double)BYTES_PER_MB,
                "GB" => bytes / (double)BYTES_PER_GB,
                "SECTORES" or "SECTORS" => bytes / (double)(sectorSize ?? DEFAULT_SECTOR_SIZE),
                "BLOQUES" or "BLOCKS" => blockSize.HasValue ? bytes / (double)blockSize.Value : throw new InvalidOperationException("Block size required for block conversion"),
                _ => throw new ArgumentException($"Unknown target unit: {targetUnit}")
            };
        }

        // Get display name for unit
        public static string GetUnitDisplayName(string unit)
        {
            return unit.ToUpper() switch
            {
                "BYTES" or "B" => "Bytes",
                "KB" => "KB",
                "MB" => "MB",
                "GB" => "GB",
                "SECTORES" or "SECTORS" => "Sectores",
                "BLOQUES" or "BLOCKS" => "Bloques",
                _ => unit
            };
        }

        // Format value with unit
        public static string FormatWithUnit(double value, string unit)
        {
            return $"{value:N2} {GetUnitDisplayName(unit)}";
        }
    }
}
