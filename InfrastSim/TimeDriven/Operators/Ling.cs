namespace InfrastSim.TimeDriven.Operators;
internal class Ling : OperatorBase {
    public override string Name => "令";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter center && !IsTired) {
            simu.DelayAction(simu => {
                foreach (var op in center.Operators) {
                    op.MoodConsumeRate.Remove("岁");
                }
            });

            if (Upgraded >= 2) {
                if (Mood >= 12) {
                    simu.Renjianyanhuo.SetValue(Name, 15);
                } else {
                    simu.Ganzhixinxi.SetValue(Name, 10);
                }
            }
        }
    }
}