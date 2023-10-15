namespace InfrastSim.TimeDriven.Operators;
internal class Dusk : OperatorBase {
    public override string Name => "夕";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter center && !IsTired) {
            center.ExtraMoodModifier.SetValue(Name, -0.05);
            MoodConsumeRate.SetValue("岁", 0.5);

            if (Mood >= 12) {
                simu.Ganzhixinxi.SetValue(Name, 10);
            } else {
                simu.Renjianyanhuo.SetValue(Name, 15);
            }
        }
    }
}
