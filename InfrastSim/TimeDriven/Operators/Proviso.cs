namespace InfrastSim.TimeDriven.Operators; 
internal class Proviso : OperatorBase {
    public override string Name => "但书";

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            trading.OnPending += OnPending;
        }
    }

    public void OnPending(OrderPendingArgs args) {
        var extraGold = Upgraded >= 2 ? 2 : 1;
        var extraRewards = extraGold * 500;

        if (Order.Gold.Contains(args.OriginOrder) && args.OriginOrder.Consumes < 4) {
            args.CurrentOrder = args.CurrentOrder with {
                Consumes = args.CurrentOrder.Consumes with { Count = args.CurrentOrder.Consumes + extraGold },
                Earns = args.CurrentOrder.Earns with { Count = args.CurrentOrder.Earns + extraRewards },
            };
        }
    }
}
