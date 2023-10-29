namespace InfrastSim.TimeDriven.Operators;
internal class Ling : OperatorBase {
    public override string Name => "令";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;
    static int[] _thresholds = new[] { 0, 12 };
    public override int[] Thresholds => _thresholds;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter center && !IsTired) {
            simu.Delay(simu => {
                var names = center.Operators.Select(op => op.Name);
                foreach (var op in center.GroupMembers("岁")) {
                    foreach (var name in names) {
                        op.MoodConsumeRate.Disable(op.Name);
                    }
                    op.MoodConsumeRate.Disable("control-center-extra");
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