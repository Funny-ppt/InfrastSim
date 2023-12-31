namespace InfrastSim.TimeDriven.Operators;
internal class Dusk : OperatorBase {
    public override string Name => "夕";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;
    static int[] _thresholds = new[] { 0, 12 };
    public override int[] Thresholds => _thresholds;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter center && !IsTired) {
            center.ExtraMoodModifier.SetValue(Name, -0.05);
            MoodConsumeRate.SetValue(Name, 0.5);

            if (MoodTicks >= 12 * 360000) {
                simu.Ganzhixinxi.SetValue(Name, 10);
            } else {
                simu.Renjianyanhuo.SetValue(Name, 15);
            }
        }
    }
}
