# File System Visualizer

Aplicación de escritorio en .NET WPF para visualizar, simular y gestionar estructuras de sistemas de archivos (FAT, Unix/EXT, NTFS).

## 📋 Descripción

Esta aplicación educativa permite a estudiantes y profesionales comprender el funcionamiento interno de diferentes sistemas de archivos mediante la configuración flexible de parámetros, la visualización de estructuras de datos y la simulación de operaciones de gestión de archivos.

## ✨ Características Principales

### 🎯 Sistemas de Archivos Soportados

- **FAT (File Allocation Table)** - FAT12, FAT16, FAT32
  - Visualización completa de la tabla FAT
  - Gestión de archivos y directorios
  - Simulación de asignación de bloques
  - Soporte para hard links
- **Unix/EXT (Extended File System)** - Con sistema de i-nodos
- **NTFS (New Technology File System)** - Con Master File Table (MFT)

### 🔧 Configuración Flexible

La aplicación está diseñada para reflejar escenarios reales de ejercicios donde **no siempre se tienen todos los datos**:

#### FAT
- ✅ Selectores de unidades (Bytes, KB, MB, GB, Sectores)
- ✅ Tamaño de disco configurable
- ✅ Tamaño de clúster/bloque configurable
- ✅ Tamaño de sector opcional (por defecto: 512 bytes)
- ✅ Auto-detección de tipo FAT (FAT12/16/32) según número de bloques
- ✅ Configuración de bloques reservados (Boot Sector)
- ✅ Configuración de número de copias de FAT
- ✅ Tamaño de entrada de directorio configurable
- ✅ Número de entradas en directorio raíz configurable
- ✅ Cálculo automático de registros por bloque (RRL/BLQ)
- ✅ **Visualización interactiva de la tabla FAT**
- ✅ **Gestión dinámica de archivos y directorios**

#### Unix/EXT
- ✅ Selectores de unidades para disco y bloques
- ✅ **Estructura de i-nodo completamente configurable**:
  - Número variable de punteros directos (10, 12, 13, etc.)
  - Punteros indirectos opcionales (simple, doble, triple)
  - Tamaño de puntero configurable (2, 4, 8 bytes / 16, 32, 64 bits)
- ✅ Número de i-nodos opcional
- ✅ Tamaño de i-nodo opcional (por defecto: 128 bytes)
- ✅ Tamaño de metadatos en i-nodo configurable
- ✅ Cálculo automático de tamaño máximo de archivo

#### NTFS
- ✅ Selectores de unidades para disco y clúster
- ✅ Tamaño de entrada MFT configurable (por defecto: 1 KB)
- ✅ Porcentaje de MFT Zone opcional (por defecto: 12.5%)
- ✅ Tamaño de MFT especificable directamente o calculado del porcentaje
- ✅ Configuración de archivos residentes vs no residentes
- ✅ Cálculo automático de número máximo de archivos

### 🎨 Interfaz de Usuario

- **Diseño moderno** con Material Design
- **Navegación intuitiva** entre pantallas
- **Validación en tiempo real** de datos ingresados
- **Tooltips explicativos** (💡) para ayuda contextual
- **Indicadores visuales** de campos opcionales vs obligatorios
- **Campos calculados automáticamente** mostrados en gris/cursiva
- **Visualización gráfica** de la estructura del disco
- **Tabla FAT interactiva** con códigos de colores

### 🎮 Gestión de Archivos (FAT)

La visualización FAT incluye un sistema completo de gestión de archivos:

- **Crear archivos** con tamaño personalizado
- **Crear directorios** (siempre ocupan 1 bloque)
- **Crear enlaces duros (hard links)** que apuntan al mismo bloque
- **Especificar bloques de inicio** manualmente o asignación automática
- **Asignación breadth-first** (por anchura) de bloques
- **Eliminación de archivos y directorios**
- **Visualización en tiempo real** de la tabla FAT
- **Etiquetado automático** de archivos (A-Z)

#### Tipos de Entrada Soportados
- **F** - Archivo regular
- **D** - Directorio (siempre 1 bloque)
- **L** - Enlace simbólico
- **H** - Hard link (comparte bloques con archivo original)

## 🏗️ Arquitectura

### Patrón MVVM (Model-View-ViewModel)

```
FileSystemVisualizer/
├── Models/                    # Modelos de datos
│   ├── UnitValue.cs          # Manejo de valores con unidades
│   ├── DataField.cs          # Campos opcionales/calculados
│   ├── FatConfigurationFlexible.cs
│   ├── FatFileSystem.cs      # Lógica del sistema FAT
│   ├── FatEntry.cs           # Entrada de archivo/directorio
│   ├── FatTableEntry.cs      # Entrada en tabla FAT
│   ├── BlockGroup.cs         # Agrupación de bloques
│   ├── UnixConfigurationFlexible.cs
│   └── NtfsConfigurationFlexible.cs
├── ViewModels/               # Lógica de presentación
│   ├── SelectionViewModel.cs
│   ├── FatInputFlexibleViewModel.cs
│   ├── FatVisualizationViewModel.cs  # Gestión de archivos FAT
│   ├── UnixInputFlexibleViewModel.cs
│   └── NtfsInputFlexibleViewModel.cs
├── Views/                    # Interfaces XAML
│   ├── SelectionView.xaml
│   ├── FatInputFlexibleView.xaml
│   ├── FatVisualizationView.xaml     # Visualización FAT
│   ├── UnixInputFlexibleView.xaml
│   └── NtfsInputFlexibleView.xaml
├── Services/                 # Servicios de la aplicación
│   └── NavigationService.cs
├── Helpers/                  # Clases auxiliares
│   ├── RelayCommand.cs
│   ├── UnitConverter.cs
│   └── InvertBooleanConverter.cs
└── Controls/                 # Controles reutilizables
    └── UnitInputControl.xaml
```

### Componentes Clave

#### UnitValue
Clase para manejar valores con diferentes unidades y conversiones automáticas:
```csharp
var diskSize = new UnitValue(100, "GB");
long bytes = diskSize.ToBytes();  // 107374182400
double mb = diskSize.ToMB();      // 102400
```

#### DataField<T>
Clase genérica para campos opcionales y calculados:
```csharp
var field = new DataField<int>();
field.SetSpecifiedValue(512);     // Usuario especificó
field.SetCalculatedValue(1024);   // Sistema calculó
bool isUserProvided = field.IsSpecified;
```

#### FatFileSystem
Clase principal que gestiona la estructura del sistema FAT:
```csharp
var fs = new FatFileSystem(config);
fs.AssignBlocksBreadthFirst(rootDirectory);
fs.GenerateFatTable();
var regions = fs.GetRegions();  // Boot, FAT, Root, Data
var blocks = fs.GetBlockGroups(); // Visualización optimizada
```

#### FatEntry
Representa un archivo o directorio en el sistema FAT:
```csharp
var file = new FatEntry
{
    Name = "documento.txt",
    Type = 'F',
    SizeInRecords = 25,
    RecordsPerBlock = 10,
    SpecificStartBlock = null  // Asignación automática
};
```

## 🚀 Instalación y Uso

### Requisitos Previos

- **.NET 9.0 SDK** o superior
- **Windows** (WPF es específico de Windows)
- **Visual Studio 2022** o **Visual Studio Code** (opcional)

### Compilación

```powershell
cd FileSystemVisualizer
dotnet build
```

### Ejecución

```powershell
dotnet run
```

O ejecutar directamente el `.exe` generado en:
```
FileSystemVisualizer/bin/Debug/net9.0-windows/FileSystemVisualizer.exe
```

## 📖 Guía de Uso

### 1. Pantalla de Selección

Al iniciar la aplicación, verás tres tarjetas:
- **FAT** (azul) - File Allocation Table
- **Unix/EXT** (rojo) - Extended File System
- **NTFS** (verde) - New Technology File System

Haz clic en la tarjeta del sistema que deseas configurar.

### 2. Configuración de Parámetros

Cada formulario tiene secciones claramente definidas:

#### Campos Obligatorios (*)
Marcados con asterisco, deben ser completados.

#### Campos Opcionales (☐)
Con checkbox "Especificar", puedes activarlos si tienes ese dato.

#### Selectores de Unidades
Cada campo numérico tiene un selector de unidad (Bytes, KB, MB, GB, etc.)

### 3. Visualización FAT

Después de configurar el sistema FAT, accederás a la pantalla de visualización que incluye:

#### Panel de Gestión de Archivos
- **Nombre**: Nombre del archivo o directorio
- **Tipo**: F (archivo), D (directorio), L (enlace simbólico), H (hard link)
- **Tamaño en Registros**: Número de registros que ocupa
- **Bloque Inicial**: Especificar manualmente o dejar en automático
- **Archivo Enlazado**: Para crear hard links, especificar el archivo original

#### Visualización de Disco
- **Bloques agrupados** por tipo (BOOT, FAT, Root, archivos)
- **Código de colores** para identificar regiones
- **Etiquetas de archivos** (A-Z)

#### Tabla FAT
- **Índice de bloque**: Número del bloque
- **Siguiente bloque**: Cadena de bloques del archivo
- **Archivo**: Etiqueta del archivo que ocupa el bloque
- **Estado**: RESERVED, FREE, EOF (-1), o número de siguiente bloque

### 4. Ejemplos de Uso

#### Ejemplo 1: Configuración FAT Básica
```
Tamaño del disco: 100 GB
Tamaño del bloque: 4 KB
Tamaño del sector: 512 Bytes (por defecto)
Tipo FAT: Auto (detectará FAT32)
Bloques reservados: 32 (FAT32)
Copias de FAT: 2
```

#### Ejemplo 2: Crear Estructura de Archivos
```
1. Crear directorio "Documentos" (Tipo: D)
2. Crear archivo "foto.jpg" (Tipo: F, Tamaño: 50 registros)
3. Crear hard link "foto_backup.jpg" (Tipo: H, Enlazado a: foto.jpg)
4. Observar cómo ambos archivos apuntan al mismo bloque inicial
```

#### Ejemplo 3: Unix con i-nodo Personalizado
```
Tamaño del disco: 500 GB
Tamaño del bloque: 4 KB
Punteros directos: 10
☑ Indirecto simple
☐ Indirecto doble (desactivado)
☐ Indirecto triple (desactivado)
Tamaño de puntero: 4 bytes (32 bits)
```

#### Ejemplo 4: NTFS con MFT Específica
```
Tamaño del disco: 1 TB
Tamaño del clúster: 4 KB
☑ Especificar tamaño de la MFT: 500 MB
Tamaño de entrada MFT: 1 KB
```

## 🧮 Cálculos Automáticos

### FAT
- **Número de bloques** = Tamaño disco / Tamaño bloque
- **Tipo FAT**:
  - ≤ 4,096 bloques → FAT12
  - ≤ 65,536 bloques → FAT16
  - > 65,536 bloques → FAT32
- **Registros por bloque (RRL/BLQ)** = Tamaño bloque / Tamaño entrada directorio
- **Tamaño de FAT** = (Bloques × Bits por entrada / 8) / Bytes por sector
- **Bloques reservados**: FAT32 = 32, FAT12/16 = 1 (por defecto)
- **Sectores del directorio raíz** = (Entradas × Tamaño entrada) / Bytes por sector

### Unix/EXT
- **Bloques totales** = Tamaño disco / Tamaño bloque
- **Punteros por bloque** = Tamaño bloque / Tamaño puntero
- **Tamaño máximo de archivo** = 
  - (Directos × Tamaño bloque) +
  - (Punteros/bloque × Tamaño bloque) [simple] +
  - (Punteros/bloque² × Tamaño bloque) [doble] +
  - (Punteros/bloque³ × Tamaño bloque) [triple]

### NTFS
- **Clústeres totales** = Tamaño disco / Tamaño clúster
- **Tamaño MFT** = Tamaño disco × (% MFT Zone / 100)
- **Archivos máximos** = Tamaño MFT / Tamaño entrada MFT

## 🎓 Casos de Uso Educativos

### Para Estudiantes
- Comprender cómo diferentes parámetros afectan la estructura del sistema de archivos
- Experimentar con configuraciones variadas
- Visualizar el impacto de decisiones de diseño
- Simular la creación y gestión de archivos
- Entender la diferencia entre enlaces duros y simbólicos
- Observar la fragmentación y asignación de bloques

### Para Profesores
- Crear ejercicios con diferentes niveles de información
- Demostrar conceptos de sistemas operativos
- Generar ejemplos para exámenes
- Mostrar visualmente cómo funcionan las tablas FAT
- Explicar la diferencia entre sistemas de archivos

## 🔄 Conversión de Unidades

La aplicación soporta conversión automática entre:
- **Bytes** (B)
- **Kilobytes** (KB) = 1,024 bytes
- **Megabytes** (MB) = 1,024² bytes
- **Gigabytes** (GB) = 1,024³ bytes
- **Sectores** (tamaño configurable, típicamente 512 bytes)
- **Bloques** (tamaño configurable según sistema)

## 📊 Estructura de la Tabla FAT

La tabla FAT muestra:
- **Bloques reservados** (BOOT, FAT, Root Directory)
- **Bloques libres** (FREE)
- **Bloques ocupados** con etiqueta del archivo (A-Z)
- **Cadenas de bloques** mostrando el siguiente bloque
- **Marcador EOF** (-1) para el último bloque de un archivo

### Códigos en la Tabla FAT
- `null` o vacío: Bloque libre
- `-1`: EOF (End of File) - Último bloque del archivo
- `-2`: Bloque reservado (Boot, FAT, Root)
- `N` (número): Índice del siguiente bloque en la cadena

## 🎨 Características de Visualización

### Agrupación de Bloques
Los bloques consecutivos del mismo tipo se agrupan para una visualización más clara:
- **BOOT**: Sector de arranque y bloques reservados
- **FAT**: Tabla(s) de asignación de archivos
- **Root Directory**: Directorio raíz (FAT12/16)
- **A-Z**: Archivos y directorios del usuario
- **FREE**: Bloques disponibles

### Información de Estructura del Disco
Muestra las regiones del disco en sectores:
- Sector de inicio
- Longitud en sectores
- Tipo de región

## 📚 Documentación Adicional

### Archivos de Referencia
- `docs/` - PDFs con teoría de sistemas de archivos
- Documentación técnica sobre FAT, Unix/EXT y NTFS

## 🐛 Solución de Problemas

### La aplicación no inicia
- Verificar que .NET 9.0 esté instalado: `dotnet --version`
- Recompilar: `dotnet clean && dotnet build`

### Errores de validación
- Todos los campos marcados con * son obligatorios
- Los valores deben ser números positivos
- Las unidades deben ser seleccionadas del dropdown

### La tabla FAT no se actualiza
- Hacer clic en el botón "Refrescar Visualización"
- Verificar que los archivos tengan tamaños válidos

### Bloques fuera de rango
- Reducir el tamaño de los archivos
- Aumentar el tamaño del disco
- Aumentar el tamaño del bloque

## 🔮 Características Implementadas

- [x] Configuración flexible de sistemas de archivos
- [x] Visualización gráfica de estructuras FAT
- [x] Gestión dinámica de archivos y directorios
- [x] Soporte para hard links
- [x] Asignación automática de bloques (breadth-first)
- [x] Tabla FAT interactiva
- [x] Cálculo automático de parámetros
- [x] Validación en tiempo real
- [x] Selectores de unidades

## 🚧 Próximas Características

- [ ] Visualización gráfica para Unix/EXT y NTFS
- [ ] Exportación a imagen/PDF
- [ ] Simulación de operaciones de lectura/escritura
- [ ] Cálculo de fragmentación
- [ ] Comparación entre sistemas de archivos
- [ ] Ejemplos predefinidos
- [ ] Modo de tutorial interactivo
- [ ] Soporte para enlaces simbólicos (soft links)
- [ ] Simulación de defragmentación

## 🛠️ Tecnologías Utilizadas

- **.NET 9.0** - Framework de desarrollo
- **WPF (Windows Presentation Foundation)** - Interfaz de usuario
- **XAML** - Diseño de interfaces
- **C#** - Lenguaje de programación
- **Material Design** - Estilo visual
- **MVVM Pattern** - Arquitectura de la aplicación

## 📝 Notas de Desarrollo

### Convenciones de Código
- Patrón MVVM estricto
- Uso de `INotifyPropertyChanged` para binding
- Comandos implementados con `RelayCommand`
- Conversiones de unidades centralizadas en `UnitConverter`

### Estructura de Datos
- `FatEntry`: Árbol jerárquico de archivos y directorios
- `FatTableEntry`: Entrada individual en la tabla FAT
- `BlockGroup`: Agrupación de bloques consecutivos para visualización
- `DiskRegion`: Región del disco (Boot, FAT, Root, Data)

## 🤝 Contribuciones

Este proyecto es parte de un trabajo académico. Las sugerencias y mejoras son bienvenidas.

## 📄 Licencia

Proyecto educativo desarrollado para la asignatura de Diseño de Sistemas Operativos.

---

**Versión**: 2.0.0 (FAT Visualization & File Management)  
**Última actualización**: Enero 2026  
**Autor**: Airam  
**Repositorio**: https://github.com/Airamsveedraaa/AppFicherosDESO
