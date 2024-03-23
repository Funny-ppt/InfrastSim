namespace InfrastSim.TimeDriven.Operators;
internal class Lancet_2 : OperatorBase {
    public override string Name => "Lancet-2";
    static string[] _groups = { "作业平台" };
    public override string[] Groups => _groups;
    public override int DormVipPriority => 0;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-65);
        }
        if (Facility is PowerStation && !IsTired) {
            EfficiencyModifier.SetValue(Name, 10);
        }
    }
}

