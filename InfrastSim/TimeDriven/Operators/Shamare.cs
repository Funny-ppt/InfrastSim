namespace InfrastSim.TimeDriven.Operators;

internal class Shamare : OperatorBase {
    public override string Name => "巫恋";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            trading.PreGoldOrderPending += args =>
                args.Priority4Gold.SetIfGreater(0.67 * Math.Min(1, WorkingTime / TimeSpan.FromHours(3)));

            if (Upgraded >= 2) {
                foreach (var op in trading.Operators) {
                    if (op != this) {
                        op.EfficiencyModifier.MaxValue = 0;
                    }
                    op.MoodConsumeRate.SetValue(Name, 0.25);
                }
                EfficiencyModifier.SetValue(Name, (trading.Operators.Count() - 1) * 0.45);
            }
        }
    }
}