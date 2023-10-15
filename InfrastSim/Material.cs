namespace InfrastSim;
internal record Material(string Name, int Count) {
    public static implicit operator int(Material mat) => mat.Count;
}
