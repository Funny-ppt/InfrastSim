namespace InfrastSim.TimeDriven.Operators;

internal class Kirara : OperatorBase {
    public override string Name => "绮良";
    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EffiencyModifier.SetValue(Name, 0.05);
            simu.ExtraGoldProductionLine.SetValue(
                Name, simu.GetRealGoldProductionLine() / (Upgraded >= 2 ? 2 : 4) * 2);
        }
    }
}