namespace InfrastSim.TimeDriven.Operators;

internal class Позёмка : OperatorBase {
    public override string Name => "鸿雪";
    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            simu.DelayAction(simu => {
                EffiencyModifier.SetValue(Name, simu.GetGoldProductionLine() * 0.05);
            });

            if (Upgraded >= 2) {
                simu.ExtraGoldProductionLine.SetValue(Name,
                    Math.Min(4, simu.OperatorsInFacility.Where(op => op.Groups.Contains("杜林")).Count()));
            }
        }
    }
}
