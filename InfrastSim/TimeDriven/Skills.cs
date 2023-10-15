namespace InfrastSim.TimeDriven; 
internal static class Skills {
    public static void 裁缝Alpha(OperatorBase op, TimeDrivenSimulator simu) {
        if (op.Facility is TradingStation trading && !op.IsTired) {
            trading.PreGoldOrderPending += args =>
                args.Priority4Gold.SetIfGreater(0.67 * Math.Min(1, op.WorkingTime / TimeSpan.FromHours(3)));
        }
    }

    public static void 裁缝Beta(OperatorBase op, TimeDrivenSimulator simu) {
        if (op.Facility is TradingStation trading && !op.IsTired) {
            trading.PreGoldOrderPending += args => {
                if (op.WorkingTime < TimeSpan.FromHours(3)) {
                    args.Priority4Gold.SetIfGreater(3 * Math.Min(1, op.WorkingTime / TimeSpan.FromHours(3)));
                } else {
                    args.Priority4Gold.SetIfGreater(1.5 + 2.5 * Math.Min(1, op.WorkingTime / TimeSpan.FromHours(5)));
                }
            };
        }
    }
}
