namespace InfrastSim.TimeDriven.Operators;
internal class Midnight : OperatorBase {
    public override string Name => "月见夜";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 30);
            MoodConsumeRate.SetValue(Name, -25);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceOriginStone()) {
                EfficiencyModifier.SetValue(Name, 30);
            }
        }
    }
}

