using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
internal class TradingStation : FacilityBase, IApplyDrones {
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
    public IEnumerable<Order> Orders => _orders.Where(o => o != null);
    public int OrderCount => Orders.Count();
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
        if (order == null) return false;
        var index = Array.IndexOf(_orders, order);
        if (index == -1) return false;
        _orders[index] = null;
        return true;
    }
    public void RemoveAllOrder() {
        Array.Fill(_orders, null);
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
    public override bool IsWorking => CapacityN > OrderCount && Operators.Any();

    public override void Reset() {
        base.Reset();

        PreGoldOrderPending = null;
        OnPending = null;
        Capacity.Clear();
    }
    void MakeProgress(Simulator simu, TimeSpan timeElapsed) {
        if (IsWorking) {
            if (CurrentOrder == null) {
                PendingNewOrder();
            }
            var effiency = 1 + TotalEffiencyModifier + simu.GlobalTradingEffiency;
            var equivTime = timeElapsed * effiency;
            if (equivTime >= RemainsTime) {
                var remains = equivTime - RemainsTime;

                InsertCurrentOrder();
                if (PendingNewOrder()) {
                    Debug.Assert(CurrentOrder != null);
                    Progress += remains / CurrentOrder.ProduceTime;
                }
            }
        }
    }

    public override void Update(Simulator simu, TimeElapsedInfo info) {
        MakeProgress(simu, info.TimeElapsed);

        base.Update(simu, info);
    }

    public void ApplyDrones(Simulator simu, int amount) {
        amount = Math.Max(amount, simu.Drones);
        MakeProgress(simu, TimeSpan.FromMinutes(3 * amount));
        simu.RemoveDrones(amount);
    }


    protected override void WriteDerivedContent(Utf8JsonWriter writer, bool detailed = false) {
        if (CurrentOrder != null) {
            writer.WriteItem("current-order", CurrentOrder, detailed);
            writer.WriteNumber("progress", Progress);

            writer.WritePropertyName("orders");
            writer.WriteStartArray();
            foreach (var order in Orders) {
                writer.WriteItemValue(order, detailed);
            }
            writer.WriteEndArray();
        }

        if (detailed) {
            writer.WriteNumber("remains", RemainsTime.TotalSeconds);
            writer.WriteNumber("base-capacity", BaseCapacity);
            writer.WriteNumber("capacity", CapacityN);
            var args = Level switch {
                1 => new GoldOrderPendingArgs(new(1), new(max: 0), new(max: 0)),
                2 => new GoldOrderPendingArgs(new(0.6), new(0.4), new(max: 0)),
                3 => new GoldOrderPendingArgs(new(0.3), new(0.5), new(0.2)),
                _ => throw new NotImplementedException()
            };
            PreGoldOrderPending?.Invoke(args);
            writer.WritePropertyName("order-chance");
            writer.WriteStartArray();
            writer.WriteNumber("2", args.Priority2Gold);
            writer.WriteNumber("3", args.Priority3Gold);
            writer.WriteNumber("4", args.Priority4Gold);
            writer.WriteEndArray();
        }
    }
    protected override void ReadDerivedContent(JsonElement elem) {
        if (elem.TryGetProperty("progress", out var progress)) {
            Progress = progress.GetDouble();
        }
        if (elem.TryGetProperty("current-order", out var order)) {
            CurrentOrder = Order.FromJson(order);
        }
    }
}
