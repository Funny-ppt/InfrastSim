namespace InfrastSim.TimeDriven.Operators;

internal class Позёмка : OperatorBase {
    public override string Name => "鸿雪";
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

                EfficiencyModifier.SetValue(Name, productLines * 0.05);
            });

            if (Upgraded >= 2) {
                simu.ExtraGoldProductionLine.SetValue(Name,
                    Math.Min(4, simu.GroupMemberCount("杜林")
                 ));
            }
        }
    }
}
