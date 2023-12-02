namespace InfrastSim.TimeDriven.Operators;

internal class Gnosis : OperatorBase {
    public override string Name => "灵知";
    static string[] _groups = { "喀兰贸易" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter control && !IsTired) {
            var amount = control.GroupMemberCount("喀兰贸易");
            control.ExtraMoodModifier.SetValue(Name, amount * -0.05);

            if (Upgraded >= 2) {
                foreach (var op in simu.TradingStations.SelectMany(t => t.WorkingOperators)) {
                    if (op.HasGroup("喀兰贸易")) {
                        op.EfficiencyModifier.SetValue(Name, -0.15);
                        ((TradingStation)op.Facility!).Capacity.AddValue(Name, 6);
                    }
                }
            }
        }
    }
}