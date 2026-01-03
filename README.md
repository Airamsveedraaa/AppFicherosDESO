# File System Visualizer

Aplicación de escritorio en .NET WPF para visualizar y simular estructuras de sistemas de archivos (FAT, Unix/EXT, NTFS).

## 📋 Descripción

Esta aplicación permite a estudiantes y profesionales comprender el funcionamiento interno de diferentes sistemas de archivos mediante la configuración flexible de parámetros y la visualización de sus estructuras de datos.

## ✨ Características Principales

### 🎯 Sistemas de Archivos Soportados

- **FAT (File Allocation Table)** - FAT12, FAT16, FAT32
- **Unix/EXT (Extended File System)** - Con sistema de i-nodos
- **NTFS (New Technology File System)** - Con Master File Table (MFT)

### 🔧 Configuración Flexible

La aplicación está diseñada para reflejar escenarios reales de ejercicios donde **no siempre se tienen todos los datos**:

#### FAT
- ✅ Selectores de unidades (Bytes, KB, MB, GB, Sectores)
- ✅ Tamaño de disco configurable
- ✅ Tamaño de clúster configurable
- ✅ Tamaño de sector opcional (por defecto: 512 bytes)
- ✅ Auto-detección de tipo FAT (FAT12/16/32) según número de bloques
- ✅ Tamaño de FAT calculado automáticamente o especificado manualmente
- ✅ Configuración de número de copias de FAT

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

## 🏗️ Arquitectura

### Patrón MVVM (Model-View-ViewModel)

```
FileSystemVisualizer/
├── Models/                    # Modelos de datos
│   ├── UnitValue.cs          # Manejo de valores con unidades
│   ├── DataField.cs          # Campos opcionales/calculados
│   ├── FatConfigurationFlexible.cs
│   ├── UnixConfigurationFlexible.cs
│   └── NtfsConfigurationFlexible.cs
├── ViewModels/               # Lógica de presentación
│   ├── SelectionViewModel.cs
│   ├── FatInputFlexibleViewModel.cs
│   ├── UnixInputFlexibleViewModel.cs
│   └── NtfsInputFlexibleViewModel.cs
├── Views/                    # Interfaces XAML
│   ├── SelectionView.xaml
│   ├── FatInputFlexibleView.xaml
│   ├── UnixInputFlexibleView.xaml
│   └── NtfsInputFlexibleView.xaml
├── Services/                 # Servicios de la aplicación
│   └── NavigationService.cs
├── Helpers/                  # Clases auxiliares
│   ├── RelayCommand.cs
│   └── UnitConverter.cs
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

#### UnitConverter
Helper estático con métodos de conversión:
```csharp
long bytes = UnitConverter.ToBytes(4, "KB");        // 4096
double gb = UnitConverter.FromBytes(bytes, "GB");   // 0.00000381469...
```

## 🚀 Instalación y Uso

### Requisitos Previos

- **.NET 10.0 SDK** o superior
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
FileSystemVisualizer/bin/Debug/net10.0-windows/FileSystemVisualizer.exe
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

### 3. Ejemplos de Uso

#### Ejemplo 1: Configuración FAT Básica
```
Tamaño del disco: 100 GB
Tamaño del clúster: 4 KB
Tipo FAT: Auto (detectará FAT32)
```

#### Ejemplo 2: Unix con i-nodo Personalizado
```
Tamaño del disco: 500 GB
Tamaño del bloque: 4 KB
Punteros directos: 10
☑ Indirecto simple
☐ Indirecto doble (desactivado)
☐ Indirecto triple (desactivado)
```

#### Ejemplo 3: NTFS con MFT Específica
```
Tamaño del disco: 1 TB
Tamaño del clúster: 4 KB
☑ Especificar tamaño de la MFT: 500 MB
```

## 🧮 Cálculos Automáticos

### FAT
- **Número de bloques** = Tamaño disco / Tamaño clúster
- **Tipo FAT**:
  - ≤ 4,096 bloques → FAT12
  - ≤ 65,536 bloques → FAT16
  - \> 65,536 bloques → FAT32
- **Tamaño de FAT** = Bloques × Bytes por entrada

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

### Para Profesores
- Crear ejercicios con diferentes niveles de información
- Demostrar conceptos de sistemas operativos
- Generar ejemplos para exámenes

## 🔄 Conversión de Unidades

La aplicación soporta conversión automática entre:
- **Bytes** (B)
- **Kilobytes** (KB) = 1,024 bytes
- **Megabytes** (MB) = 1,024² bytes
- **Gigabytes** (GB) = 1,024³ bytes
- **Sectores** (tamaño configurable, típicamente 512 bytes)
- **Bloques** (tamaño configurable según sistema)

## 📚 Documentación Adicional

### Archivos de Referencia
- `docs/` - PDFs con teoría de sistemas de archivos
- Documentación técnica en carpeta `brain/`

## 🐛 Solución de Problemas

### La aplicación no inicia
- Verificar que .NET 10.0 esté instalado: `dotnet --version`
- Recompilar: `dotnet clean && dotnet build`

### Errores de validación
- Todos los campos marcados con * son obligatorios
- Los valores deben ser números positivos
- Las unidades deben ser seleccionadas del dropdown

## 🔮 Próximas Características

- [ ] Visualización gráfica de estructuras de archivos
- [ ] Exportación a imagen/PDF
- [ ] Simulación de operaciones de archivos
- [ ] Cálculo de fragmentación
- [ ] Comparación entre sistemas de archivos
- [ ] Ejemplos predefinidos
- [ ] Modo de tutorial interactivo

---

**Versión**: 1.0.0 (Flexible Input Forms)  
**Última actualización**: Enero 2026
