using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemVisualizer.Models
{
    public class FatFileSystem
    {
        public FatConfigurationFlexible Configuration { get; private set; }

        // Disk Geometry
        public long TotalSectors { get; private set; }
        public int BytesPerSector { get; private set; }
        public int SectorsPerCluster { get; private set; }
        public long BytesPerCluster { get; private set; }

        // Layout Regions (in Sectors)
        public long ReservedSectors { get; private set; }
        public long FatSizeSectors { get; private set; }
        public long RootDirSectors { get; private set; }
        public long DataStartSector { get; private set; }
        public long TotalClusters { get; private set; }

        // Detailed Region Info
        public long BootSectorStart => 0; 
        public long Fat1Start => ReservedSectors;
        public long Fat2Start => Fat1Start + FatSizeSectors;
        public long RootDirStart => Fat2Start + FatSizeSectors;
        public long DataStart => RootDirStart + RootDirSectors;

        // File Management
        public List<FatEntry> Files { get; set; } = new List<FatEntry>();
        public List<FatTableEntry> FatTable { get; private set; } = new List<FatTableEntry>();

        public FatFileSystem(FatConfigurationFlexible config)
        {
            Configuration = config;
            CalculateLayout();
        }

        private void CalculateLayout()
        {
            if (Configuration.DiskSize.Value == null || Configuration.ClusterSize.Value == null)
                return;

            BytesPerSector = (int)(Configuration.SectorSize.Value?.ToBytes() ?? 512);
            long diskSizeBytes = (long)Configuration.DiskSize.Value.ToBytes();
            TotalSectors = diskSizeBytes / BytesPerSector;

            long clusterSizeBytes = (long)Configuration.ClusterSize.Value.ToBytes();
            BytesPerCluster = clusterSizeBytes;
            SectorsPerCluster = (int)(clusterSizeBytes / BytesPerSector);

            // 1. Reserved Blocks (Boot Sector + others)
            // Use value from Configuration if specified, otherwise default based on FAT type
            string fatType = Configuration.DeterminedFatType;
            long reservedBlocks;
            if (Configuration.ReservedBlocks.IsSpecified)
            {
                reservedBlocks = Configuration.ReservedBlocks.Value;
            }
            else
            {
                // FAT12/16 usually 1 block, FAT32 usually more
                reservedBlocks = fatType == "FAT32" ? 32 : 1;
            }
            // Convert blocks to sectors
            ReservedSectors = reservedBlocks * SectorsPerCluster;

            // 2. Root Directory Sectors (FAT12/16 only)
            // FAT32 stores Root Dir in Data Area
            if (fatType == "FAT32")
            {
                RootDirSectors = 0;
            }
            else
            {
                int rootEntries = Configuration.RootDirectoryEntries.Value;
                int entrySize = Configuration.DirectoryEntrySize.Value; // Usually 32 bytes
                long rootDirBytes = (long)rootEntries * entrySize;
                RootDirSectors = (rootDirBytes + BytesPerSector - 1) / BytesPerSector;
            }

            // 3. FAT Size
            // We need to calculate how many sectors the FAT tables take.
            // This is circular because FAT size depends on TotalClusters, and TotalClusters depends on Data Area size which depends on FAT size.
            // Simplified approach: Estimate clusters based on disk size, then refine.
            
            // Available sectors for FATs + Data = Total - Reserved - RootDir
            long availableSectors = TotalSectors - ReservedSectors - RootDirSectors;
            
            // Equation: Available = (NumFats * FatSectors) + (NumClusters * SectorsPerCluster)
            // FatSectors = (NumClusters * BitsPerEntry / 8) / BytesPerSector
            
            int numFats = Configuration.NumberOfFatCopies.Value;
            double bitsPerEntry = fatType == "FAT12" ? 12 : (fatType == "FAT16" ? 16 : 32);
            
            // Approximate formula to solve for NumClusters:
            // Available = NumClusters * ( (NumFats * BitsPerEntry / 8 / BytesPerSector) + SectorsPerCluster )
            
            double bytesPerFatEntry = bitsPerEntry / 8.0;
            double fatOverheadPerCluster = (numFats * bytesPerFatEntry) / BytesPerSector;
            
            double estimatedClusters = availableSectors / (fatOverheadPerCluster + SectorsPerCluster);
            
            // Calculate FAT size from estimated clusters
            long fatSizeBytes = (long)Math.Ceiling(estimatedClusters * bytesPerFatEntry);
             // Align to sector boundary
            FatSizeSectors = (fatSizeBytes + BytesPerSector - 1) / BytesPerSector;

            // 4. Final Calculations
            DataStartSector = ReservedSectors + (numFats * FatSizeSectors) + RootDirSectors;
            long dataSectors = TotalSectors - DataStartSector;
            TotalClusters = dataSectors / SectorsPerCluster;
        }

        public List<DiskRegion> GetRegions()
        {
            var regions = new List<DiskRegion>();
            
            regions.Add(new DiskRegion("Boot Sector / Reserved", BootSectorStart, ReservedSectors, "Boot"));
            
            int numFats = Configuration.NumberOfFatCopies.Value;
            for (int i = 0; i < numFats; i++)
            {
                long start = Fat1Start + (i * FatSizeSectors);
                regions.Add(new DiskRegion($"FAT {i + 1}", start, FatSizeSectors, "FAT"));
            }

            if (RootDirSectors > 0)
            {
                regions.Add(new DiskRegion("Root Directory", RootDirStart, RootDirSectors, "Root"));
            }

            regions.Add(new DiskRegion("Data Area", DataStart, TotalClusters * SectorsPerCluster, "Data"));

            return regions;
        }

        /// <summary>
        /// Asigna bloques a los archivos usando algoritmo breadth-first (por anchura)
        /// </summary>
        public void AssignBlocksBreadthFirst(FatEntry rootDirectory)
        {
            if (rootDirectory == null) return;

            // Calcular primer bloque de datos disponible
            long reservedBlocks = DataStart / SectorsPerCluster;
            int currentBlock = (int)reservedBlocks;

            // Cola para recorrido por anchura
            Queue<FatEntry> queue = new Queue<FatEntry>();
            queue.Enqueue(rootDirectory);

            while (queue.Count > 0)
            {
                var entry = queue.Dequeue();

                // Asignar bloque inicial
                if (entry.SpecificStartBlock.HasValue)
                {
                    // Bloque específico indicado por el usuario
                    entry.StartBlock = entry.SpecificStartBlock.Value;
                }
                else
                {
                    // Asignación automática
                    entry.StartBlock = currentBlock;
                    currentBlock += entry.SizeInBlocks;
                }

                // Añadir hijos a la cola (breadth-first)
                foreach (var child in entry.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Genera la tabla FAT basándose en los archivos definidos
        /// </summary>
        public void GenerateFatTable()
        {
            FatTable.Clear();

            // Generar layout visual basado en bloques físicos
            long totalDiskBlocks = TotalSectors / SectorsPerCluster;
            
            // Inicializar todos los bloques
            for (int i = 0; i < totalDiskBlocks; i++)
            {
                FatTable.Add(new FatTableEntry
                {
                    BlockIndex = i,
                    NextBlock = null, // FREE por defecto
                    FileLabel = null,
                    IsReserved = false
                });
            }

            // Definir límites de regiones en BLOQUES
            long bootBlocks = Configuration.ReservedBlocks.Value; // Usar valor configurado directamente
            long fatSizeBlocks = (FatSizeSectors + SectorsPerCluster - 1) / SectorsPerCluster;
            long fatStartBlock = bootBlocks;
            long rootStartBlock = fatStartBlock + (fatSizeBlocks * Configuration.NumberOfFatCopies.GetValueOrDefault());
            long rootSizeBlocks = (RootDirSectors + SectorsPerCluster - 1) / SectorsPerCluster;
            long dataStartBlock = rootStartBlock + rootSizeBlocks;

            // Marcar regiones reservadas (Boot, FAT, Root)
            for (int i = 0; i < totalDiskBlocks; i++)
            {
                var entry = FatTable[i];
                if (i < dataStartBlock)
                {
                    entry.IsReserved = true;
                    entry.NextBlock = -2; // Marcar como reservado en la tabla FAT
                }
            }

            // Aplanar el árbol de archivos
            var allFiles = FlattenFileTree(Files);

            // Asignar etiquetas a los archivos (A-Z)
            char currentLabel = 'A';
            foreach (var file in allFiles)
            {
                file.FileLabel = currentLabel;
                
                // Crear cadena de bloques para este archivo
                for (int i = 0; i < file.SizeInBlocks; i++)
                {
                    int blockIndex = file.StartBlock + i;
                    
                    if (blockIndex >= TotalClusters)
                        break; // Fuera de rango

                    FatTable[blockIndex].FileLabel = currentLabel;
                    
                    // Establecer siguiente bloque
                    if (i == file.SizeInBlocks - 1)
                    {
                        FatTable[blockIndex].NextBlock = -1; // EOF
                    }
                    else
                    {
                        FatTable[blockIndex].NextBlock = file.StartBlock + i + 1;
                    }
                }

                currentLabel++;
                if (currentLabel > 'Z') currentLabel = 'A'; // Wrap around
            }
        }

        /// <summary>
        /// Aplana el árbol de archivos en una lista
        /// </summary>
        private List<FatEntry> FlattenFileTree(List<FatEntry> files)
        {
            var result = new List<FatEntry>();
            foreach (var file in files)
            {
                result.Add(file);
                if (file.Children.Count > 0)
                {
                    result.AddRange(FlattenFileTree(file.Children.ToList()));
                }
            }
            return result;
        }

        /// <summary>
        /// Agrupa bloques consecutivos del mismo tipo para visualización optimizada
        /// </summary>
        public List<BlockGroup> GetBlockGroups()
        {
            var groups = new List<BlockGroup>();
            if (FatTable.Count == 0) return groups;

            BlockGroup? currentGroup = null;

            for (int i = 0; i < FatTable.Count; i++)
            {
                var entry = FatTable[i];
                string type = entry.IsReserved ? "RESERVED" : 
                             (entry.NextBlock == null ? "FREE" : 
                             (entry.FileLabel?.ToString() ?? "?"));

                if (currentGroup == null || currentGroup.Type != type)
                {
                    // Iniciar nuevo grupo
                    if (currentGroup != null)
                        groups.Add(currentGroup);

                    string fileName = null;
                    
                    // Asignar nombres descriptivos a regiones reservadas
                    if (entry.IsReserved)
                    {
                        // Usar directamente los bloques reservados configurados
                        long bootBlocks = Configuration.ReservedBlocks.Value;
                        long fatSizeBlocks = (FatSizeSectors + SectorsPerCluster - 1) / SectorsPerCluster;
                        long fatStart = bootBlocks;
                        long rootStart = fatStart + (fatSizeBlocks * Configuration.NumberOfFatCopies.GetValueOrDefault());
                        
                        if (i < bootBlocks)
                            fileName = "BOOT";
                        else if (i < rootStart)
                            fileName = i < fatStart + fatSizeBlocks ? "FAT" : "Copia FAT";
                        else
                            fileName = "Root Directory";
                    }
                    else if (entry.FileLabel.HasValue)
                    {
                        fileName = Files.FirstOrDefault(f => f.FileLabel == entry.FileLabel)?.Name;
                    }

                    currentGroup = new BlockGroup
                    {
                        StartBlock = i,
                        EndBlock = i,
                        Type = type,
                        FileName = fileName
                    };
                }
                else
                {
                    // Extender grupo actual
                    currentGroup.EndBlock = i;
                }
            }

            // Añadir último grupo
            if (currentGroup != null)
                groups.Add(currentGroup);

            return groups;
        }
    }

    public class DiskRegion
    {
        public string Name { get; set; }
        public long StartSector { get; set; }
        public long LengthSectors { get; set; }
        public string Type { get; set; } // "Boot", "FAT", "Root", "Data"

        public long EndSector => StartSector + LengthSectors - 1;

        public DiskRegion(string name, long start, long length, string type)
        {
            Name = name;
            StartSector = start;
            LengthSectors = length;
            Type = type;
        }
    }
}
