namespace InfrastSim.TimeDriven.Operators;
internal class Bena : OperatorBase {
    public override string Name => "贝娜";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, -0.2);
            MoodConsumeRate.SetValue(Name, -0.25);
            manufacturing.Capacity.SetValue(Name, 17);
        }
        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            MoodConsumeRate.SetValue(Name, -0.25);
            trading.PreGoldOrderPending += args =>
                args.Priority4Gold.SetIfGreater(0.67 * Math.Min(1, WorkingTime / TimeSpan.FromHours(3)));
        }
    }
}
