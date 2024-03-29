namespace InfrastSim.TimeDriven.Operators;
internal class Haze : OperatorBase {
    public override string Name => "夜烟";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && manufacturing.IsProduceGold() && !IsTired) {
            EfficiencyModifier.SetValue(30);
        }

        if (Facility is TradingStation trading && !IsTired && Upgraded >= 1) {
            EfficiencyModifier.SetValue(Name, 30);
        }
    }
}
