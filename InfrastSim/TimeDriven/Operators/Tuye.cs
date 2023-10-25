namespace InfrastSim.TimeDriven.Operators;

internal class Tuye : OperatorBase {
    public override string Name => "图耶";
    static string[] _groups = { "虚拟赤金线" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            simu.Delay(simu => {
                EfficiencyModifier.SetValue(
                    Name, simu.GetGoldProductionLine() / (Upgraded >= 2 ? 2 : 4) * 0.15 + 0.05);
            });
        }
    }
}