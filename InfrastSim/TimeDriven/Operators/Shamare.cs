namespace InfrastSim.TimeDriven.Operators;

internal class Shamare : OperatorBase {
    public override string Name => "巫恋";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            MoodConsumeRate.SetValue(Name, -0.25);
            trading.PreGoldOrderPending += args =>
                args.Priority4Gold.SetIfGreater(0.67 * Math.Min(1, WorkingTime / TimeSpan.FromHours(3)));

            if (Upgraded >= 2) {
                simu.Delay(simu => {
                    foreach (var op in trading.Operators) {
                        if (op != this) {
                            op.EfficiencyModifier.MinValue = 0;
                            op.EfficiencyModifier.MaxValue = 0;
                        }
                        op.MoodConsumeRate.SetValue(Name, 0.25);
                    }
                    EfficiencyModifier.SetValue(Name, (trading.Operators.Count() - 1) * 0.45);
                }, Priority.Shamare);
            }
        }
    }
}