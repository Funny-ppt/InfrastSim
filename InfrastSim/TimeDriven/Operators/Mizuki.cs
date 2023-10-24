namespace InfrastSim.TimeDriven.Operators;

internal class Mizuki : OperatorBase {
    public override string Name => "水月";
    static string[] _groups2 = { "标准化" };
    public override string[] Groups => Upgraded >= 2 ? _groups2 : base.Groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            var count = manufacturing.GroupMemberCount("标准化");
            EfficiencyModifier.SetValue(Name, count * 0.05);

            if (Upgraded >= 2) {
                EfficiencyModifier.AddValue(Name, 0.25);
            }
        }
    }
}