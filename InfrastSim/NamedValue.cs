namespace InfrastSim;
public struct NamedValue {
    public string Name;
    public double Value;

    public NamedValue(string name, double value) {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
    }

    public static implicit operator NamedValue(double value) {
        return new("common", value);
    }
    public static implicit operator double(NamedValue value) {
        return value.Value;
    }
}
