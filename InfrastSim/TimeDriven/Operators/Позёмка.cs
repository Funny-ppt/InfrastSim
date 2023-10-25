namespace InfrastSim.TimeDriven.Operators;

internal class Позёмка : OperatorBase {
    public override string Name => "鸿雪";
    static string[] _groups = { "虚拟赤金线" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            simu.Delay(simu => {
                EfficiencyModifier.SetValue(Name, simu.GetGoldProductionLine() * 0.05);
            });

            if (Upgraded >= 2) {
                simu.ExtraGoldProductionLine.SetValue(Name,
                    Math.Min(4, simu.GroupMemberCount("杜林")
                 ));
            }
        }
    }
}
