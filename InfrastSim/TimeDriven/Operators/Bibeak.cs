namespace InfrastSim.TimeDriven.Operators;

internal class Bibeak : OperatorBase {
    public override string Name => "柏喙";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            MoodConsumeRate.SetValue(Name, -25);
            if (Upgraded < 2) {
                trading.PreGoldOrderPending += args =>
                    args.Priority4Gold.SetIfGreater((int)(67 * Math.Min(1, WorkingTime.TotalHours / 3)));
            } else {
                trading.PreGoldOrderPending += args => {
                    if (WorkingTime < TimeSpan.FromHours(3)) {
                        args.Priority4Gold.SetIfGreater((int)(300 * Math.Min(1, WorkingTime.TotalHours / 3)));
                    } else {
                        args.Priority4Gold.SetIfGreater((int)(150 + 250 * Math.Min(1, WorkingTime.TotalHours / 5)));
                    }
                };
            }
        }
    }
}