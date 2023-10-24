namespace InfrastSim.TimeDriven.Operators;
internal class Dorothy : OperatorBase {
    public override string Name => "多萝西";
    static string[] _groups0 = { "莱茵生命" };
    static string[] _groups2 = { "莱茵生命", "莱茵科技制造" };
    public override string[] Groups => Upgraded >= 2 ? _groups2 : _groups0;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            var count = manufacturing.GroupMemberCount("莱茵科技制造");
            EfficiencyModifier.SetValue(Name, 0.05 * count + (Upgraded >= 2 ? 0.25 : 0));
        }
    }
}