namespace InfrastSim.TimeDriven.Operators;

internal class Ashlock : OperatorBase {
    public override string Name => "灰毫";
    static string[] _groups = { "红松骑士团" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 0.25 : 0.15);
        }
    }
}