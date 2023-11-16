namespace InfrastSim.TimeDriven.Operators;
internal class Yato : OperatorBase {
    public override string Name => "夜刀";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.3);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.15);
        }
    }
}

