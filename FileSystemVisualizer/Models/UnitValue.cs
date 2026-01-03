using System;

namespace FileSystemVisualizer.Models
{
    public class UnitValue
    {
        public double Value { get; set; }
        public string Unit { get; set; } = "KB"; // Default unit

        public UnitValue() { }

        public UnitValue(double value, string unit)
        {
            Value = value;
            Unit = unit;
        }

        // Convert to bytes
        public long ToBytes(int? sectorSize = null, int? blockSize = null)
        {
            return Unit.ToUpper() switch
            {
                "BYTES" or "B" => (long)Value,
                "KB" => (long)(Value * 1024),
                "MB" => (long)(Value * 1024 * 1024),
                "GB" => (long)(Value * 1024 * 1024 * 1024),
                "SECTORES" or "SECTORS" => sectorSize.HasValue ? (long)(Value * sectorSize.Value) : throw new InvalidOperationException("Sector size required"),
                "BLOQUES" or "BLOCKS" => blockSize.HasValue ? (long)(Value * blockSize.Value) : throw new InvalidOperationException("Block size required"),
                _ => throw new ArgumentException($"Unknown unit: {Unit}")
            };
        }

        // Convert to KB
        public double ToKB(int? sectorSize = null, int? blockSize = null)
        {
            return ToBytes(sectorSize, blockSize) / 1024.0;
        }

        // Convert to MB
        public double ToMB(int? sectorSize = null, int? blockSize = null)
        {
            return ToBytes(sectorSize, blockSize) / (1024.0 * 1024.0);
        }

        // Convert to GB
        public double ToGB(int? sectorSize = null, int? blockSize = null)
        {
            return ToBytes(sectorSize, blockSize) / (1024.0 * 1024.0 * 1024.0);
        }

        // Convert to another unit
        public UnitValue ConvertTo(string targetUnit, int? sectorSize = null, int? blockSize = null)
        {
            var bytes = ToBytes(sectorSize, blockSize);
            
            double newValue = targetUnit.ToUpper() switch
            {
                "BYTES" or "B" => bytes,
                "KB" => bytes / 1024.0,
                "MB" => bytes / (1024.0 * 1024.0),
                "GB" => bytes / (1024.0 * 1024.0 * 1024.0),
                "SECTORES" or "SECTORS" => sectorSize.HasValue ? bytes / (double)sectorSize.Value : throw new InvalidOperationException("Sector size required"),
                "BLOQUES" or "BLOCKS" => blockSize.HasValue ? bytes / (double)blockSize.Value : throw new InvalidOperationException("Block size required"),
                _ => throw new ArgumentException($"Unknown target unit: {targetUnit}")
            };

            return new UnitValue(newValue, targetUnit);
        }

        public override string ToString()
        {
            return $"{Value} {Unit}";
        }
    }
}
