namespace InfrastSim.TimeDriven.Operators;
internal class Tequila : OperatorBase {
    public override string Name => "龙舌兰";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            MoodConsumeRate.AddValue(Name, -25);
            trading.OnPending += OnPending;
        }
    }

    public void OnPending(OrderPendingArgs args) {
        var extraRewards = Upgraded >= 2 ? 500 : 250;

        if (Order.Gold.Contains(args.OriginOrder) && args.OriginOrder.Consumes == 4) {
            args.CurrentOrder = args.CurrentOrder with {
                Earns = args.CurrentOrder.Earns with { Count = args.CurrentOrder.Earns + extraRewards },
            };
        }
    }
}
