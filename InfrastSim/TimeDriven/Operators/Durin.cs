namespace InfrastSim.TimeDriven.Operators;

internal class Durin : OperatorBase {
    public override string Name => "杜林";
    static string[] _groups = { "杜林" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            MoodConsumeRate.AddValue(Name, 10);
            dorm.SetDormMoodModifier(-25);
        }
    }
}
