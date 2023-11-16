namespace InfrastSim.TimeDriven.Operators;
internal class Steward : OperatorBase {
    public override string Name => "史都华德";
    static string[] _groups1 = { "标准化" };
    public override string[] Groups => Upgraded >= 1 ? _groups1 : base.Groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            MoodConsumeRate.SetValue(Name, -0.25);
            trading.Capacity.SetValue(Name, 5);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            EfficiencyModifier.SetValue(Name, 0.25);
        }
    }
}

