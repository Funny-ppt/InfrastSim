namespace InfrastSim.TimeDriven.Operators;
internal class Kroos : OperatorBase {
    public override string Name => "克洛丝";

    static TimeSpan[] _wtThresholds = {
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(3),
        TimeSpan.FromHours(4),
        TimeSpan.FromHours(5)
    };
    public override TimeSpan[] WorkingTimeThresholds => _wtThresholds;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 15 + WorkingTime.HoursNotExceed(5) * 2);
        }
        if (Facility is Dormitory dorm && Upgraded >= 1) {
            MoodConsumeRate.AddValue(Name, -70);
        }
    }
}

