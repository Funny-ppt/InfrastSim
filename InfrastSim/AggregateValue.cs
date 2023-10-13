namespace InfrastSim;

internal class AggregateValue {
    double _baseValue;
    double _value = new();
    readonly Dictionary<string, double> _additionValues = new();
    bool _upToDate = false;

    public AggregateValue(double baseValue) {
        _baseValue = baseValue;
    }

    public bool UpToDate {
        get => _upToDate;
    }
    public double CalculatedValue {
        get {
            if (_upToDate) return _value;
            _upToDate = true;
            return _value = _baseValue + _additionValues.Sum(p => p.Value);
        }
    }
    public double BaseValue => _baseValue;

    public double GetValue(string name)
        => _additionValues.GetValueOrDefault(name);
    public double SetValue(string name, double value) {
        var oldValue = GetValue(name);
        if (Util.Equals(value, oldValue)) {
            return oldValue;
        }
        _upToDate = false;
        return _additionValues[name] = value;
    }
    public double AddValue(string name, double value) {
        return SetValue(name, value + GetValue(name));
    }
    public double SetMaxValue(string name, double value) {
        return SetValue(name, Math.Max(value, GetValue(name)));
    }
    public double SetMinValue(string name, double value) {
        return SetValue(name, Math.Min(value, GetValue(name)));
    }
    public double SetValue(NamedValue namedValue) {
        return SetValue(namedValue.Name, namedValue.Value);
    }
    public double AddValue(NamedValue namedValue) {
        return AddValue(namedValue.Name, namedValue.Value);
    }
    public double SetMaxValue(NamedValue namedValue) {
        return SetMaxValue(namedValue.Name, namedValue.Value);
    }
    public double SetMinValue(NamedValue namedValue) {
        return SetMinValue(namedValue.Name, namedValue.Value);
    }
    public void Clear() {
        _upToDate = true;
        _value = _baseValue;
        _additionValues.Clear();
    }

    public static implicit operator double(AggregateValue aggregateValue) {
        return aggregateValue.CalculatedValue;
    }
}
