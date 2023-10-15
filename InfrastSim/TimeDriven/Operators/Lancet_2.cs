namespace InfrastSim.TimeDriven.Operators;
internal class Lancet_2 : OperatorBase {
    public Lancet_2() { DormVipPriority = 0; }
    public override string Name => "Lancet-2";
    static string[] _groups = { "作业平台" };
    public override string[] Groups => _groups;

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.GetVip()?.MoodConsumeRate.SetIfLesser("dorm-vip", -0.65);
        }
        if (Facility is PowerStation && !IsTired) {
            EffiencyModifier.SetValue(Name, 0.1);
        }
    }
}

