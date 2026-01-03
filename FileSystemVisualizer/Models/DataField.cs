namespace FileSystemVisualizer.Models
{
    public class DataField<T>
    {
        private T? _value;

        public T? Value
        {
            get => _value;
            set
            {
                _value = value;
                if (value != null && !IsSpecified)
                {
                    IsCalculated = true;
                }
            }
        }

        public bool IsSpecified { get; set; }
        public bool IsCalculated { get; set; }
        public T? DefaultValue { get; set; }

        public DataField()
        {
            IsSpecified = false;
            IsCalculated = false;
        }

        public DataField(T defaultValue)
        {
            DefaultValue = defaultValue;
            _value = defaultValue;
            IsSpecified = false;
            IsCalculated = false;
        }

        public T GetValueOrDefault()
        {
            if (Value != null)
                return Value;
            
            if (DefaultValue != null)
                return DefaultValue;
            
            return default(T)!;
        }

        public void SetSpecifiedValue(T value)
        {
            Value = value;
            IsSpecified = true;
            IsCalculated = false;
        }

        public void SetCalculatedValue(T value)
        {
            Value = value;
            IsSpecified = false;
            IsCalculated = true;
        }

        public void Clear()
        {
            Value = DefaultValue;
            IsSpecified = false;
            IsCalculated = false;
        }
    }
}
