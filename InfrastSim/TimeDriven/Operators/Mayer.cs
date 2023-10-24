namespace InfrastSim.TimeDriven.Operators;

internal class Mayer : OperatorBase {
    public override string Name => "梅尔";
    static string[] _groups = { "莱茵生命" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1

        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 2) {
            EfficiencyModifier.SetValue(0.3);
        }
    }
}
