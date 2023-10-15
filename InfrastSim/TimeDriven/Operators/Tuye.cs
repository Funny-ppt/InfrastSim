namespace InfrastSim.TimeDriven.Operators;

internal class Tuye : OperatorBase {
    public override string Name => "图耶";
    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            simu.DelayAction(simu => {
                EffiencyModifier.SetValue(
                    Name, simu.GetGoldProductionLine() / (Upgraded >= 2 ? 2 : 4) * 0.15 + 0.05);
            });
        }
    }
}