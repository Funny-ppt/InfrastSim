namespace InfrastSim.TimeDriven.Operators;

internal class Tuye : OperatorBase {
    public override string Name => "图耶";
    static string[] _groups = { "虚拟赤金线" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            simu.Delay(simu => {
                var productLines = simu.GetGoldProductionLine();
                var kirara = trading.Operators.FirstOrDefault(op => op is Kirara);
                if (kirara != null && trading.IndexOf(this) < trading.IndexOf(kirara)) {
                    productLines -= simu.GetRealGoldProductionLine() / (kirara.Upgraded >= 2 ? 2 : 4) * 2;
                }

                EfficiencyModifier.SetValue(
                    Name, productLines / (Upgraded >= 2 ? 2 : 4) * 15 + 5);
            });
        }
    }
}