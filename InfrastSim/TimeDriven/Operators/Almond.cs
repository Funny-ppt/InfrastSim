namespace InfrastSim.TimeDriven.Operators;
internal class Almond : OperatorBase {
    public override string Name => "杏仁";
    static string[] _groups = { "黑钢国际" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            MoodConsumeRate.SetValue(Name, 25);
            if (manufacturing.Product == Product.Gold) {
                EfficiencyModifier.SetValue(Name, 25);

                if (Upgraded >= 2) {
                    var count = simu.GroupMemberCount("黑钢国际");
                    count = Math.Min(count, 3);
                    EfficiencyModifier.AddValue(Name, count * 2);
                    MoodConsumeRate.AddValue(Name, count * -15);
                }
            }
        }
    }
}
