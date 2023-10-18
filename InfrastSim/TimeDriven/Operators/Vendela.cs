namespace InfrastSim.TimeDriven.Operators;
internal class Vendela : OperatorBase {
    public override string Name => "刺玫";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.DormMoodModifier.SetIfLesser(-0.15);

            if (Upgraded >= 2) {
                foreach (var op in Facility.Operators) {
                    if (op.Mood <= 18) {
                        op.MoodConsumeRate.SetIfLesser("芬芳疗养beta", -0.1);
                    }
                }
            }
        }
    }
}
