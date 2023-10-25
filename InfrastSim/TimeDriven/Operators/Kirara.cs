namespace InfrastSim.TimeDriven.Operators;

internal class Kirara : OperatorBase {
    public override string Name => "绮良";
    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.05);

            if (trading.HasGroupMember("虚拟赤金线")) {
                simu.Delay(simu => {
                    simu.ExtraGoldProductionLine.SetValue(
                        Name, simu.GetRealGoldProductionLine() / (Upgraded >= 2 ? 2 : 4) * 2);
                });
            } else {
                simu.ExtraGoldProductionLine.SetValue(
                    Name, simu.GetRealGoldProductionLine() / (Upgraded >= 2 ? 2 : 4) * 2);
            }
        }
    }
}