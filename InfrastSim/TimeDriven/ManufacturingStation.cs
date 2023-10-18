using System.Diagnostics;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
internal class ManufacturingStation : FacilityBase, IApplyDrones {
    public override FacilityType Type => FacilityType.Manufacturing;

    public int BaseCapacity => Level switch {
        1 => 24,
        2 => 36,
        3 => 54,
        _ => 0,
    };
    public AggregateValue Capacity { get; private set; } = new AggregateValue(0, 1, 1000);
    public int CapacityN => (int)Capacity;
    public int CapacityOccupied => Product == null ? 0 : ProductCount * Product.Volume;
    public int ProductCount { get; internal set; } = 0;
    public bool CanStoreMore => Product != null && CapacityN - CapacityOccupied >= Product.Volume;
    public Product? Product { get; private set; }
    public double Progress { get; private set; }
    public TimeSpan RemainsTime => (Product?.ProduceTime ?? TimeSpan.MaxValue) * (1 - Progress);
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
    public override bool IsWorking => CanStoreMore && Operators.Any();

    public override void Reset() {
        base.Reset();

        Capacity.Clear();
    }
    public override void Resolve(Simulator simu) {
        Capacity.SetValue("base", BaseCapacity);

        base.Resolve(simu);
    }
    void MakeProgress(Simulator simu, TimeSpan timeElapsed) {
        if (IsWorking) {
            var effiency = 1 + TotalEffiencyModifier + simu.GlobalManufacturingEffiency;
            var equivTime = timeElapsed * effiency;
            if (equivTime >= RemainsTime) {
                var remains = equivTime - RemainsTime;

                ProductCount += 1;
                if (CanStoreMore) {
                    Debug.Assert(Product != null);
                    Progress += remains / Product.ProduceTime;
                }
            } else {
                Progress += equivTime / Product!.ProduceTime;
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
        writer.WriteNumber("product-index", Array.IndexOf(Product.AllProducts, Product));
        writer.WriteNumber("progress", Progress);
        writer.WriteNumber("product-count", ProductCount);

        if (detailed) {
            writer.WriteString("product", Product!.Name);
            writer.WriteNumber("remains", RemainsTime.TotalSeconds);
            writer.WriteNumber("base-capacity", BaseCapacity);
            writer.WriteNumber("capacity", CapacityN);
        }
    }
    protected override void ReadDerivedContent(JsonElement elem) {
        if (elem.TryGetProperty("progress", out var progress)) {
            Progress = progress.GetDouble();
        }
        if (elem.TryGetProperty("product-index", out var productIndex)) {
            var index = productIndex.GetInt32();
            if (index >= 0 && index < Product.AllProducts.Length) {
                Product = Product.AllProducts[index];
            }
        }
        if (elem.TryGetProperty("product-count", out var productCount)) {
            ProductCount = productCount.GetInt32();
        }
    }
}
