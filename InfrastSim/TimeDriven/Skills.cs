namespace InfrastSim.TimeDriven; 
internal static class Skills {
    public static void 裁缝Alpha(OperatorBase op, Simulator simu) {
        if (op.Facility is TradingStation trading && !op.IsTired) {
            trading.PreGoldOrderPending += args =>
                args.Priority4Gold.SetIfGreater((int)(67 * Math.Min(1, op.WorkingTime.TotalHours / 3)));
        }
    }

    public static void 裁缝Beta(OperatorBase op, Simulator simu) {
        if (op.Facility is TradingStation trading && !op.IsTired) {
            trading.PreGoldOrderPending += args => {
                if (op.WorkingTime < TimeSpan.FromHours(3)) {
                    args.Priority4Gold.SetIfGreater((int)(300 * Math.Min(1, op.WorkingTime.TotalHours / 3)));
                } else {
                    args.Priority4Gold.SetIfGreater((int)(150 + 250 * Math.Min(1, op.WorkingTime.TotalHours / 5)));
                }
            };
        }
    }
}
