namespace InfrastSim.TimeDriven.Operators;

internal class Bibeak : OperatorBase {
    public override string Name => "柏喙";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            MoodConsumeRate.SetValue(Name, -0.25);
            if (Upgraded < 2) {
                trading.PreGoldOrderPending += args =>
                    args.Priority4Gold.SetIfGreater(0.67 * Math.Min(1, WorkingTime / TimeSpan.FromHours(3)));
            } else {
                trading.PreGoldOrderPending += args => {
                    if (WorkingTime < TimeSpan.FromHours(3)) {
                        args.Priority4Gold.SetIfGreater(3 * Math.Min(1, WorkingTime / TimeSpan.FromHours(3)));
                    } else {
                        args.Priority4Gold.SetIfGreater(1.5 + 2.5 * Math.Min(1, WorkingTime / TimeSpan.FromHours(5)));
                    }
                };
            }
        }
    }
}