namespace InfrastSim.TimeDriven.Operators;

internal class Viviana : OperatorBase {
    public override string Name => "薇薇安娜";
    static string[] _groups = { "骑士" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter control && !IsTired) {
            control.ExtraMoodModifier.SetValue(Name, 0.05);

            if (Upgraded >= 2) {
                var ops = simu.ManufacturingStations.SelectMany(fac => fac.WorkingOperators);
                foreach (var op in ops) {
                    if (op.HasGroup("骑士")) {
                        op.EfficiencyModifier.AddValue(Name, 0.07);
                    }
                }
            }
        }
    }
}