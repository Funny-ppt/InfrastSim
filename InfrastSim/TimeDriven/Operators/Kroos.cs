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
            var time = Math.Min(5, Math.Floor(WorkingTime / TimeSpan.FromHours(1)));
            EfficiencyModifier.SetValue(Name, 0.15 + time * 0.02);
        }
        if (Facility is Dormitory dorm && Upgraded >= 1) {
            MoodConsumeRate.AddValue(Name, -0.7);
        }
    }
}

