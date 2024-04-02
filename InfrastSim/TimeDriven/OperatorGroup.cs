namespace InfrastSim.TimeDriven;
internal record struct OperatorGroup(string GroupName, int GroupUpgraded = 0) {
    public static implicit operator OperatorGroup(string group) => new(group);
    public static implicit operator OperatorGroup((string name, int upgraded) tuple) => new(tuple.name, tuple.upgraded);
}
