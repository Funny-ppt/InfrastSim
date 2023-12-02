namespace InfrastSim.TimeDriven.Operators;

internal class Flametail : OperatorBase {
    public override string Name => "焰尾";
    static string[] _groups = { "红松骑士团" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter control && !IsTired) {
            control.ExtraMoodModifier.SetValue(Name, -0.05);

            if (Upgraded >= 2) {
                foreach (var op in simu.ManufacturingStations.SelectMany(t => t.WorkingOperators)) {
                    if (op.HasGroup("红松骑士团")) {
                        var manufacturing = (ManufacturingStation)op.Facility!;
                        if (manufacturing.IsProduceCombatRecord()) {
                            op.EfficiencyModifier.SetValue(Name, 0.1);
                        }
                        if (manufacturing.IsProduceGold()) {
                            op.EfficiencyModifier.SetValue(Name, -0.1);
                        }
                    }
                }
            }
        }
    }
}