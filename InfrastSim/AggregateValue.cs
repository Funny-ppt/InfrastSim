using System.Text.Json;

namespace InfrastSim;

public class AggregateValue(int baseValue = 0, int min = int.MinValue, int max = int.MaxValue, bool displayAsPercentage = false) : IJsonSerializable {
    int _baseValue = baseValue;
    int _value = 0;
    readonly Dictionary<string, int> _additionValues = [];
    readonly HashSet<string> _disables = [];
    bool _upToDate = false;

    public int CalculatedValue {
        get {
            if (_upToDate) return _value;
            _upToDate = true;
            var additions = _additionValues
                .ExceptBy(_disables, p => p.Key)
                .Sum(p => p.Value);
            return _value = Math.Clamp(_baseValue + additions, MinValue, MaxValue);
        }
    }
    public int BaseValue => _baseValue;
    public int MinValue { get; set; } = min;
    public int MaxValue { get; set; } = max;

    public IEnumerable<KeyValuePair<string, int>> EnumerateValues() {
        return _additionValues
            .ExceptBy(_disables, p => p.Key);
    }

    public int GetValue(string name) => _additionValues.GetValueOrDefault(name);
    public int SetValue(string name, int value) {
        var oldValue = GetValue(name);
        if (value == oldValue)
            return oldValue;
        if (!_disables.Contains(name))
            _upToDate = false;
        return _additionValues[name] = value;
    }
    public int AddValue(string name, int value) {
        return SetValue(name, value + GetValue(name));
    }
    public int SetIfGreater(string name, int value) {
        return SetValue(name, Math.Max(value, GetValue(name)));
    }
    public int SetIfLesser(string name, int value) {
        return SetValue(name, Math.Min(value, GetValue(name)));
    }
    public int SetValue(int value) => SetValue("common", value);
    public int AddValue(int value) => AddValue("common", value);
    public int SetIfGreater(int value) => SetIfGreater("common", value);
    public int SetIfLesser(int value) => SetIfLesser("common", value);

    public void Disable(string name) {
        if (_disables.Add(name)) {
            if (GetValue(name) != 0) _upToDate = false;
        }
    }
    public void Enable(string name) {
        if (_disables.Remove(name)) {
            if (GetValue(name) != 0) _upToDate = false;
        }
    }
    public void Remove(string name) {
        if (GetValue(name) != 0) {
            _upToDate = false;
        }
        _additionValues.Remove(name);
    }
    public void Clear() {
        _disables.Clear();
        // BENCHMARK REQUIRED: use Clear() or manually set value to 0?
        _additionValues.Clear();
        //foreach (var key in _additionValues.Keys) { 
        //    _additionValues[key] = 0;
        //}
        _upToDate = true;
        _value = _baseValue;
    }

    public void ToJson(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteStartObject();
        if (displayAsPercentage) {
            writer.WriteNumber("value", CalculatedValue / 100.0);
            writer.WriteNumber("base-value", BaseValue / 100.0);
            writer.WriteNumber("min-value", MinValue / 100.0);
            writer.WriteNumber("max-value", MaxValue / 100.0);
        } else {
            writer.WriteNumber("value", CalculatedValue);
            writer.WriteNumber("base-value", BaseValue);
            writer.WriteNumber("min-value", MinValue);
            writer.WriteNumber("max-value", MaxValue);
        }

        writer.WritePropertyName("details");
        writer.WriteStartArray();
        foreach (var kvp in _additionValues) {
            writer.WriteStartObject();
            writer.WriteString("tag", kvp.Key);
            if (displayAsPercentage) {
                writer.WriteNumber("value", kvp.Value / 100.0);
            } else {
                writer.WriteNumber("value", kvp.Value);
            }
            writer.WriteBoolean("disabled", _disables.Contains(kvp.Key));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    public static implicit operator int(AggregateValue aggregateValue) {
        return aggregateValue.CalculatedValue;
    }
}
