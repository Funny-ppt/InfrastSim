namespace InfrastSim;
public struct NamedValue(string name, int value) {
    public string Name = name ?? throw new ArgumentNullException(nameof(name));
    public int Value = value;

    public static implicit operator NamedValue(int value) {
        return new("common", value);
    }
    public static implicit operator int(NamedValue value) {
        return value.Value;
    }
}
