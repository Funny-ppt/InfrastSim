namespace InfrastSim.TimeDriven.Operators;
internal class Vendela : OperatorBase {
    public override string Name => "刺玫";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetDormMoodModifier(-15);

            if (Upgraded >= 2) {
                foreach (var op in Facility.Operators) {
                    if (op.MoodTicks <= 18 * 360000) {
                        op.MoodConsumeRate.SetIfLesser("芬芳疗养beta", -10);
                    }
                }
            }
        }
    }
}
