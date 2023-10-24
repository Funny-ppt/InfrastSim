namespace InfrastSim.TimeDriven.Operators;
internal class Jessica : OperatorBase {
    public override string Name => "杰西卡";
    static string[] _groups = { "黑钢国际", "标准化" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.25);
        }

        // TODO: missing skill 2
    }
}
