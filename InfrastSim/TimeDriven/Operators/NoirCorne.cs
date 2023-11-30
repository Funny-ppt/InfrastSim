namespace InfrastSim.TimeDriven.Operators;
internal class NoirCorne : OperatorBase {
    public override string Name => "黑角";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.1);
            manufacturing.Capacity.SetValue(Name, 10);
        }
        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.1);
            trading.Capacity.SetValue(Name, 2);
        }
    }
}

