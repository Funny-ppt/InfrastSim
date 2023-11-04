namespace InfrastSim.TimeDriven.Operators;

internal class Jaye : OperatorBase {
    public override string Name => "孑";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            if (Upgraded == 0) {
                simu.Delay(simu => {
                    var diff = trading.Capacity - trading.OrderCount;
                    EfficiencyModifier.AddValue(Name, diff * 0.04);
                });
            } else {
                // FIXME: 孑的技能和不同干员的交互仍然需要重点关注
                simu.Delay(simu => {
                    var total = trading.Operators.Sum(op => op.EfficiencyModifier);
                    trading.Capacity.SetValue(Name, Math.Floor(total / 0.1));
                    EfficiencyModifier.SetValue(Name, trading.Capacity * 0.04);
                }, 130);
            }
        }
    }
}
