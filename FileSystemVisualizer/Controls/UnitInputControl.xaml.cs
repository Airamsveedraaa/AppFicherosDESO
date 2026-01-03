using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FileSystemVisualizer.Controls
{
    public partial class UnitInputControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(UnitInputControl),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectedUnitProperty =
            DependencyProperty.Register(nameof(SelectedUnit), typeof(string), typeof(UnitInputControl),
                new FrameworkPropertyMetadata("KB", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AvailableUnitsProperty =
            DependencyProperty.Register(nameof(AvailableUnits), typeof(List<string>), typeof(UnitInputControl),
                new PropertyMetadata(new List<string> { "Bytes", "KB", "MB", "GB" }));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string SelectedUnit
        {
            get => (string)GetValue(SelectedUnitProperty);
            set => SetValue(SelectedUnitProperty, value);
        }

        public List<string> AvailableUnits
        {
            get => (List<string>)GetValue(AvailableUnitsProperty);
            set => SetValue(AvailableUnitsProperty, value);
        }

        public UnitInputControl()
        {
            InitializeComponent();
            
            // Bind properties
            ValueTextBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding(nameof(Value))
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });

            UnitComboBox.SetBinding(ComboBox.ItemsSourceProperty, new System.Windows.Data.Binding(nameof(AvailableUnits))
            {
                Source = this
            });

            UnitComboBox.SetBinding(ComboBox.SelectedItemProperty, new System.Windows.Data.Binding(nameof(SelectedUnit))
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.TwoWay
            });
        }
    }
}
