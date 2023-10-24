namespace InfrastSim.TimeDriven.Operators;
internal class Muelsyse : OperatorBase {
    public override string Name => "缪尔塞思";
    static string[] _groups = { "莱茵生命" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            MoodConsumeRate.SetValue(Name, -0.55);
            dorm.DormMoodModifier.SetIfLesser(-0.1);
        }
        if (Facility is PowerStation && !IsTired) {
            var amount = simu.GroupMemberCount("莱茵生命");
            EfficiencyModifier.SetValue(Name, 0.1 + Math.Min(0.15, 0.03 * amount));
        }
    }
}
