namespace InfrastSim;

internal class AggregateValue {
    double _baseValue;
    double _value = new();
    readonly Dictionary<string, double> _additionValues = new();
    readonly HashSet<string> _disables = new();
    bool _upToDate = false;

    public AggregateValue(double baseValue = 0.0, double min = double.MinValue, double max = double.MaxValue) {
        _baseValue = baseValue;
        MinValue = min;
        MaxValue = max;
    }

    public bool UpToDate {
        get => _upToDate;
    }
    public double CalculatedValue {
        get {
            if (_upToDate) return _value;
            _upToDate = true;
            var additions = _additionValues
                .ExceptBy(_disables, p => p.Key)
                .Sum(p => p.Value);
            return _value = Math.Clamp(_baseValue + additions, MinValue, MaxValue);
        }
    }
    public double BaseValue => _baseValue;
    public double MinValue { get; set; }
    public double MaxValue { get; set; }

    public double GetValue(string name)
        => _additionValues.GetValueOrDefault(name);
    public double SetValue(string name, double value) {
        var oldValue = GetValue(name);
        if (Util.Equals(value, oldValue)) {
            return oldValue;
        }
        if (!_disables.Contains(name)) _upToDate = false;
        return _additionValues[name] = value;
    }
    public double AddValue(string name, double value) {
        return SetValue(name, value + GetValue(name));
    }
    public double SetIfGreater(string name, double value) {
        return SetValue(name, Math.Max(value, GetValue(name)));
    }
    public double SetIfLesser(string name, double value) {
        return SetValue(name, Math.Min(value, GetValue(name)));
    }
    public double SetValue(NamedValue namedValue) {
        return SetValue(namedValue.Name, namedValue.Value);
    }
    public double AddValue(NamedValue namedValue) {
        return AddValue(namedValue.Name, namedValue.Value);
    }
    public double SetIfGreater(NamedValue namedValue) {
        return SetIfGreater(namedValue.Name, namedValue.Value);
    }
    public double SetIfLesser(NamedValue namedValue) {
        return SetIfLesser(namedValue.Name, namedValue.Value);
    }

    public void Disable(string name) {
        if (!_disables.Contains(name)) {
            if (!Util.Equals(GetValue(name), 0)) _upToDate = false;
            _disables.Add(name);
        }
    }
    public void Enable(string name) {
        if (_disables.Contains(name)) {
            if (!Util.Equals(GetValue(name), 0)) _upToDate = false;
            _disables.Remove(name);
        }
    }

    public void Remove(string name) {
        if (!Util.Equals(GetValue(name), 0)) {
            _upToDate = false;
        }
        _additionValues.Remove(name);
    }
    public void Clear() {
        _disables.Clear();
        foreach (var key in _additionValues.Keys) {
            _additionValues[key] = 0;
        }
        _upToDate = true;
        _value = _baseValue;
    }

    public static implicit operator double(AggregateValue aggregateValue) {
        return aggregateValue.CalculatedValue;
    }
}
