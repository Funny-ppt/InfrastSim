namespace InfrastSim.TimeDriven.Operators;
internal class Fang : OperatorBase {
    public override string Name => "芬";

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
            EfficiencyModifier.SetValue(Name, 0.2 + time * 0.01);
        }
        if (Facility is TradingStation trading && !IsTired && Upgraded >= 1) {
            EfficiencyModifier.SetValue(Name, 0.3);
        }
    }
}

