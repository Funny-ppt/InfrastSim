namespace InfrastSim.TimeDriven.Operators;
internal class Popukar : OperatorBase {
    public override string Name => "泡普卡";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.25);
            manufacturing.Capacity.SetValue(Name, -12);
            MoodConsumeRate.SetValue(Name, 0.25);
        }
        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-0.4);
            MoodConsumeRate.SetValue(Name, -0.2);
        }
    }
}

