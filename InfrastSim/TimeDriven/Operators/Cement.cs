namespace InfrastSim.TimeDriven.Operators;

internal class Cement : OperatorBase {
    public override string Name => "洋灰";
    static string[] _groups = { "标准化" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.15);

            if (Upgraded >= 2) {
                MoodConsumeRate.SetValue(Name, -0.25);
                manufacturing.Capacity.SetValue(Name, 10);
            }
        }
    }
}