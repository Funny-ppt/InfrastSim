namespace InfrastSim.TimeDriven.Operators;
internal class JessicaTheLiberated : OperatorBase {
    public override string Name => "涤火杰西卡";
    static string[] _groups = { "异格" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.1);
            trading.Capacity.SetValue(Name, 4);
        }

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired && Upgraded >= 2) {
            MoodConsumeRate.SetValue(Name, 0.5);
            var ops = simu.ManufacturingStations.SelectMany(manufacturing => manufacturing.Operators);
            foreach (var op in ops.Where(op => op.HasGroup("黑钢国际"))) {
                op.EfficiencyModifier.SetValue(Name, 0.05);
            }
        }
    }
}
