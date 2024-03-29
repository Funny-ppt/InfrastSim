namespace InfrastSim.TimeDriven.Operators;
internal class Orchid : OperatorBase {
    public override string Name => "梓兰";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Office office && !IsTired) {
            EfficiencyModifier.SetValue(Name, 40);
        }
        if (Facility is TradingStation trading && !IsTired && Upgraded >= 1) {
            EfficiencyModifier.SetValue(Name, 25);
            trading.Capacity.SetValue(Name, 1);
        }
    }
}

