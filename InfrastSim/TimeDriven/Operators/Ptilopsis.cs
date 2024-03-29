namespace InfrastSim.TimeDriven.Operators;

internal class Ptilopsis : OperatorBase {
    public override string Name => "白面鸮";

    static string[] _groups = { "莱茵生命", "莱茵科技制造" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 25 : 15);
        }
    }
}