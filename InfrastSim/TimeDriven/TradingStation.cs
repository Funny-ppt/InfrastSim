using RandomEx;
using System.Diagnostics;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public class TradingStation : FacilityBase, IApplyDrones {
    public override FacilityType Type => FacilityType.Trading;
    public int BaseCapacity => Level switch {
        1 => 6,
        2 => 8,
        3 => 10,
        _ => 0,
    };
    public AggregateValue Capacity { get; private set; } = new AggregateValue(0, 1, 64);

    Order?[] _orders = new Order[64];
    public IEnumerable<Order> Orders => _orders.Where(o => o != null);
    public int OrderCount => Orders.Count();
    public enum OrderStrategy {
        Gold,
        OriginStone,
    }
    public OrderStrategy Strategy { get; set; } = OrderStrategy.Gold;
    public Order? CurrentOrder { get; set; }
    public int Progress { get; private set; }
    public int RemainTicks => (CurrentOrder?.ProduceTicks ?? int.MaxValue) - Progress;
    public event Action<GoldOrderPendingArgs>? PreGoldOrderPending;
    public event Action<OrderPendingArgs>? OnPending;
    public bool PendingNewOrder(XoshiroRandom random) {
        if (Capacity == OrderCount) return false;

        Order order;
        if (Strategy == OrderStrategy.Gold) {
            var args = Level switch {
                1 => new GoldOrderPendingArgs(new(100), new(max: 0), new(max: 0)),
                2 => new GoldOrderPendingArgs(new(60), new(40), new(max: 0)),
                3 => new GoldOrderPendingArgs(new(30), new(50), new(20)),
                _ => throw new NotImplementedException()
            };
            PreGoldOrderPending?.Invoke(args);
            var total = args.Priority2Gold + args.Priority3Gold + args.Priority4Gold;
            var rnd = random.NextDouble() * total;
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
        if (order == null) return false;
        var index = Array.IndexOf(_orders, order);
        if (index == -1) return false;
        _orders[index] = null;
        return true;
    }
    public void RemoveAllOrder() {
        Array.Fill(_orders, null);
    }


    public override int MoodConsumeModifier => Math.Min(0, -5 * (WorkingOperatorsCount - 1));
    public override int EffiencyModifier => WorkingOperatorsCount;
    public override int AcceptOperatorNums => Level;
    public override bool IsWorking => Capacity > OrderCount && Operators.Any();

    public override void Reset() {
        base.Reset();

        PreGoldOrderPending = null;
        OnPending = null;
        Capacity.Clear();
    }
    public override void Resolve(Simulator simu) {
        Capacity.SetValue("base", BaseCapacity);

        base.Resolve(simu);
    }
    public override void QueryInterest(Simulator simu) {
        var efficiency = 100 + TotalEffiencyModifier + simu.GlobalTradingEfficiency;
        var remainSeconds = (RemainTicks + efficiency - 1) / efficiency;
        simu.SetInterest(this, remainSeconds);

        base.QueryInterest(simu);
    }
    public override void Update(Simulator simu, TimeElapsedInfo info) {
        if (IsWorking) {
            if (CurrentOrder == null) {
                if (!PendingNewOrder(simu.Random)) {
                    return;
                }
            }
            Debug.Assert(CurrentOrder != null);
            var efficiency = 100 + TotalEffiencyModifier + simu.GlobalTradingEfficiency;
            var pendingProgress = info.TimeElapsed.TotalSeconds() * efficiency;
            simu.AddTradProgress(pendingProgress);
            if (pendingProgress >= RemainTicks) {
                pendingProgress -= RemainTicks;

                InsertCurrentOrder();
                PendingNewOrder(simu.Random);
            }
            Progress += pendingProgress;
        }

        base.Update(simu, info);
    }

    public int ApplyDrones(Simulator simu, int amount) {
        if (CurrentOrder == null) return 0;

        var max = (RemainTicks + TicksHelper.TicksPerDrone - 1) / TicksHelper.TicksPerDrone;
        amount = Math.Min(Math.Min(amount, simu.Drones), max);
        var pendingProgress = amount * TicksHelper.TicksPerDrone;

        if (pendingProgress >= RemainTicks) {
            InsertCurrentOrder();
            PendingNewOrder(simu.Random);
        } else {
            Progress += amount * TicksHelper.TicksPerDrone;
        }
        simu.RemoveDrones(amount);
        return amount;
    }


    protected override void WriteDerivedContent(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteItem("current-order", CurrentOrder, detailed);
        writer.WriteNumber("progress", Progress); ;
        writer.WriteString("strategy", Strategy.ToString());
        writer.WritePropertyName("orders");
        writer.WriteStartArray();
        foreach (var order in Orders) {
            writer.WriteItemValue(order, detailed);
        }
        writer.WriteEndArray();

        if (detailed) {
            writer.WriteNumber("remains", (RemainTicks + TicksHelper.TicksPerSecond - 1) / TicksHelper.TicksPerSecond);
            writer.WriteNumber("base-capacity", BaseCapacity);
            writer.WriteNumber("capacity", Capacity);
            writer.WriteItem("capacity-details", Capacity);
            var args = Level switch {
                1 => new GoldOrderPendingArgs(new(100), new(max: 0), new(max: 0)),
                2 => new GoldOrderPendingArgs(new(60), new(40), new(max: 0)),
                3 => new GoldOrderPendingArgs(new(30), new(50), new(20)),
                _ => throw new NotImplementedException()
            };
            PreGoldOrderPending?.Invoke(args);
            writer.WritePropertyName("order-chance");
            writer.WriteStartObject();
            writer.WriteNumber("2", args.Priority2Gold);
            writer.WriteNumber("3", args.Priority3Gold);
            writer.WriteNumber("4", args.Priority4Gold);
            writer.WriteEndObject();
        }
    }
    protected override void ReadDerivedContent(JsonElement elem) {
        if (elem.TryGetProperty("strategy", out var strategy)) {
            Strategy = strategy.GetString() switch {
                "Gold" => OrderStrategy.Gold,
                "OriginStone" => OrderStrategy.OriginStone,
                _ => OrderStrategy.Gold
            };
        }
        if (elem.TryGetProperty("current-order", out var currentOrder)) {
            CurrentOrder = Order.FromJson(currentOrder);
        }
        if (elem.TryGetProperty("orders", out var orders)) {
            foreach (var orderElem in orders.EnumerateArray()) {
                var order = Order.FromJson(orderElem);
                var index = Array.IndexOf(_orders, null);
                _orders[index] = order;
            }
        }
        if (elem.TryGetProperty("progress", out var progress)) {
            if (progress.TryGetDouble(out var value)) {
                if (CurrentOrder != null) Progress = (int)(CurrentOrder.ProduceTicks * (1 - value));
            } else {
                Progress = progress.GetInt32();
            }
        }
    }
}
