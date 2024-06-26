namespace InfrastSim.TimeDriven.Operators;

internal class SilenceTheParadigmatic : OperatorBase {
    public override string Name => "淬羽赫默";
    static string[] _groups = { "莱茵科技制造", "异格" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 2) {
            EfficiencyModifier.SetValue(Name, 30);
        }
    }
}
