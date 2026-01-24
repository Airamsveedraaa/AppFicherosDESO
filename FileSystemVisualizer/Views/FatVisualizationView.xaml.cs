using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using FileSystemVisualizer.ViewModels;

namespace FileSystemVisualizer.Views
{
    public partial class FatVisualizationView : Page
    {
        public FatVisualizationView(FatVisualizationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }

    public class RegionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                switch (type)
                {
                    case "Boot": return new SolidColorBrush(Color.FromRgb(255, 179, 186)); // Pastel Red
                    case "FAT": return new SolidColorBrush(Color.FromRgb(255, 223, 186)); // Pastel Orange
                    case "Root": return new SolidColorBrush(Color.FromRgb(255, 255, 186)); // Pastel Yellow
                    case "Data": return new SolidColorBrush(Color.FromRgb(186, 255, 201)); // Pastel Green
                    default: return Brushes.LightGray;
                }
            }
            return Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
