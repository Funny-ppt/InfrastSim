using System.Diagnostics;

namespace InfrastSim.TimeDriven;
internal class TradingStation : FacilityBase {
    public override FacilityType Type => FacilityType.Trading;
    public int BaseCapacity => Level switch {
        1 => 6,
        2 => 8,
        3 => 10,
        _ => 0,
    };
    public AggregateValue Capacity { get; private set; } = new AggregateValue(0, 1, 64);
    public int CapacityN => (int)Capacity;

    Order?[] _orders = new Order[64];
    public int OrderCount => _orders.Where(o => o != null).Count();
    public enum OrderStrategy {
        Gold,
        OriginStone,
    }
    public OrderStrategy Strategy { get; set; } = OrderStrategy.Gold;
    public Order? CurrentOrder { get; set; }
    public double Progress { get; private set; }
    public TimeSpan RemainsTime => (CurrentOrder?.ProduceTime ?? TimeSpan.MaxValue) * (1 - Progress);
    public event Action<GoldOrderPendingArgs>? PreGoldOrderPending;
    public event Action<OrderPendingArgs>? OnPending;
    public bool PendingNewOrder() {
        if (CapacityN == OrderCount) return false;

        Order order;
        if (Strategy == OrderStrategy.Gold) {
            var args = Level switch {
                1 => new GoldOrderPendingArgs(new(1), new(max: 0), new(max: 0)),
                2 => new GoldOrderPendingArgs(new(0.6), new(0.4), new(max: 0)),
                3 => new GoldOrderPendingArgs(new(0.3), new(0.5), new(0.2)),
                _ => throw new NotImplementedException()
            };
            PreGoldOrderPending?.Invoke(args);
            var total = args.Priority2Gold + args.Priority3Gold + args.Priority4Gold;
            var rnd = Random.Shared.NextDouble() * total;
            if (rnd < args.Priority2Gold) {
                order = Order.Gold[0];
            } else if (rnd < args.Priority2Gold + args.Priority3Gold) {
                order = Order.Gold[1];
            } else {
                order = Order.Gold[2];
            }
        } else {
            if (Level < 3) throw new InvalidOperationException("制造原石碎片的制造站必须为3级");
            order = Order.OriginStone;
        }
        var args1 = new OrderPendingArgs(order);
        OnPending?.Invoke(args1);
        CurrentOrder = args1.CurrentOrder;
        Progress = 0;
        return true;
    }
    void InsertCurrentOrder() {
        var index = Array.IndexOf(_orders, null);
        _orders[index] = CurrentOrder;
        CurrentOrder = null;
    }
    public bool RemoveOrder(Order order) {
        var index = Array.IndexOf(_orders, order);
        if (index == -1) return false;
        _orders[index] = null;
        return true;
    }


    public override double MoodConsumeModifier {
        get {
            return Math.Min(0.0, -0.05 * (WorkingOperatorsCount - 1));
        }
    }
    public override double EffiencyModifier {
        get {
            return 0.01 * WorkingOperatorsCount;
        }
    }

    public override int AcceptOperatorNums => Level;
    public override bool IsWorking => CapacityN > OrderCount && CurrentOrder != null && Operators.Any();

    public override void Reset() {
        base.Reset();

        Capacity.Clear();
    }
    public override void Update(TimeDrivenSimulator simu, TimeElapsedInfo info) {
        if (CurrentOrder == null) {
            PendingNewOrder();
        }
        if (IsWorking) {
            var effiency = 1 + TotalEffiencyModifier + simu.GlobalTradingEffiency;
            var equivTime = info.TimeElapsed * effiency;
            if (equivTime >= RemainsTime) {
                var remains = equivTime - RemainsTime;

                InsertCurrentOrder();
                if (PendingNewOrder()) {
                    Debug.Assert(CurrentOrder != null);
                    Progress += remains / CurrentOrder.ProduceTime;
                }
            }
        }

        base.Update(simu, info);
    }
}
