namespace InfrastSim.TimeDriven.Operators;
internal class Venilla : OperatorBase {
    public override string Name => "香草";
    static string[] _groups = { "黑钢国际", "标准化" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 25);
        }

        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            EfficiencyModifier.SetValue(Name, 20);
        }
    }
}
