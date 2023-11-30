namespace InfrastSim.TimeDriven.Operators;
internal class Melantha : OperatorBase {
    public override string Name => "玫兰莎";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.25);
            trading.Capacity.SetValue(Name, 1);
        }

        // TODO: missing skill 2
    }
}

