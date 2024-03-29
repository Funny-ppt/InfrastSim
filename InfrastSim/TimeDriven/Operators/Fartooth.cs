namespace InfrastSim.TimeDriven.Operators;

internal class Fartooth : OperatorBase {
    public override string Name => "远牙";
    static string[] _groups = { "红松骑士团" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            MoodConsumeRate.SetValue(Name, -55);
            dorm.SetDormMoodModifier(-10);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 25 : 15);
        }
    }
}