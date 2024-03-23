using System.Text.Json;

namespace InfrastSim.TimeDriven;
public class ManufacturingStation : FacilityBase, IApplyDrones {
    public override FacilityType Type => FacilityType.Manufacturing;

    public int BaseCapacity => Level switch {
        1 => 24,
        2 => 36,
        3 => 54,
        _ => 0,
    };
    public AggregateValue Capacity { get; private set; } = new AggregateValue(0, 1, 1000);
    public int CapacityOccupied => Product == null ? 0 : ProductCount * Product.Volume;
    public int ProductCount { get; internal set; } = 0;
    public bool CanStoreMore => Product != null && Capacity - CapacityOccupied >= Product.Volume;
    public Product? Product { get; private set; }
    public int Progress { get; private set; }
    public int RemainTicks => (Product?.ProduceTicks ?? int.MaxValue) - Progress;
    public void ChangeProduct(Product newProduct) {
        if (newProduct != Product) {
            if (newProduct.RequiredLevel > Level) {
                throw new InvalidOperationException(
                    $"制造 {newProduct.Name} 需要{newProduct.RequiredLevel}级制造站, 当前制造站为{Level}级");
            }

            ProductCount = 0;
            Progress = 0;
            Product = newProduct;
        }
    }

    public override int MoodConsumeModifier => Math.Min(0, -5 * (WorkingOperatorsCount - 1));
    public override int EffiencyModifier => WorkingOperatorsCount;
    public override int AcceptOperatorNums => Level;
    public override bool IsWorking => CanStoreMore && Operators.Any();

    public override void Reset() {
        base.Reset();

        Capacity.Clear();
    }
    public override void Resolve(Simulator simu) {
        Capacity.SetValue("base", BaseCapacity);

        base.Resolve(simu);
    }
    public override void QueryInterest(Simulator simu) {
        var efficiency = 100 + TotalEffiencyModifier + simu.GlobalManufacturingEfficiency;
        var remainSeconds = (RemainTicks + efficiency - 1) / efficiency;
        simu.SetInterest(this, remainSeconds);

        base.QueryInterest(simu);
    }
    public override void Update(Simulator simu, TimeElapsedInfo info) {
        if (IsWorking) {
            var efficiency = 100 + TotalEffiencyModifier + simu.GlobalManufacturingEfficiency;
            var pendingProgress = info.TimeElapsed.TotalSeconds() * efficiency;
            Progress += pendingProgress;
            simu.AddManuProgress(pendingProgress);
            ProductCount = Math.Min(Capacity, ProductCount + Progress / Product!.ProduceTicks); // IsWorking == true  =>  product is not null
            if (CanStoreMore)
                Progress %= Product.ProduceTicks;
            else
                Progress = 0;
        }
        base.Update(simu, info);
    }
    public int ApplyDrones(Simulator simu, int amount) {
        if (Product == null) return 0;

        // 计算过程可能超出int上限，将第一个参数提升到long
        var max = ((long)Product.ProduceTicks * (Capacity / Product.Volume - ProductCount) - Progress + TicksHelper.TicksPerDrone - 1) / TicksHelper.TicksPerDrone;
        amount = Math.Min(amount, Math.Min((int)max, simu.Drones));

        Progress += amount * TicksHelper.TicksPerDrone;
        ProductCount += Progress / Product.ProduceTicks;
        if (CanStoreMore)
            Progress %= Product.ProduceTicks;
        else
            Progress = 0;

        simu.RemoveDrones(amount);
        return amount;
    }


    protected override void WriteDerivedContent(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteNumber("product-index", Array.IndexOf(Product.AllProducts, Product));
        writer.WriteNumber("product-count", ProductCount);
        writer.WriteNumber("progress", Progress);

        if (detailed) {
            if (Product != null) writer.WriteString("product", Product.Name);
            else writer.WriteNull("product");
            writer.WriteNumber("remains", (RemainTicks + TicksHelper.TicksPerSecond - 1) / TicksHelper.TicksPerSecond);
            writer.WriteNumber("base-capacity", BaseCapacity);
            writer.WriteNumber("capacity", Capacity);
            writer.WriteItem("capacity-details", Capacity);
        }
    }
    protected override void ReadDerivedContent(JsonElement elem) {
        if (elem.TryGetProperty("product-index", out var productIndex)) {
            var index = productIndex.GetInt32();
            if (index >= 0 && index < Product.AllProducts.Length) {
                Product = Product.AllProducts[index];
            }
        }
        if (elem.TryGetProperty("product-count", out var productCount)) {
            ProductCount = productCount.GetInt32();
        }
        if (elem.TryGetProperty("progress", out var progress)) {
            if (progress.TryGetDouble(out var value)) {
                if (Product != null) Progress = (int)(Product.ProduceTicks * (1 - value));
            } else {
                Progress = progress.GetInt32();
            }
        }
    }
}
