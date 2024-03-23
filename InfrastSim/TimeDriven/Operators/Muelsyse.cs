namespace InfrastSim.TimeDriven.Operators;
internal class Muelsyse : OperatorBase {
    public override string Name => "缪尔赛思";
    static string[] _groups = { "莱茵生命" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            MoodConsumeRate.SetValue(Name, -55);
            dorm.SetDormMoodModifier(-10);
        }
        if (Facility is PowerStation && !IsTired) {
            var amount = simu.GroupMemberCount("莱茵生命") - 1;
            EfficiencyModifier.SetValue(Name, 10 + Math.Min(15, 3 * amount));
        }
    }
}
