namespace InfrastSim.TimeDriven.Operators;

internal class Silence : OperatorBase {
    public override string Name => "赫默";
    static string[] _groups = { "莱茵科技制造" };
    public override string[] Groups => _groups;

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EffiencyModifier.SetValue(Name, Upgraded >= 2 ? 0.25 : 0.15);
        }
    }
}
