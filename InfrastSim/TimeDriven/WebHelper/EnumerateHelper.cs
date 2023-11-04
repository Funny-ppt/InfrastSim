using System.Collections.Concurrent;
using System.Text.Json;

namespace InfrastSim.TimeDriven.WebHelper;
public static class EnumerateHelper {
    static readonly OperatorBase TestOp;
    static readonly JsonSerializerOptions Options;
    static readonly Dictionary<string, Efficiency> SingleEfficiency;
    static readonly ConcurrentBag<(OpEnumData[] comb, Efficiency eff)> Results;
    static Efficiency Baseline;

    static EnumerateHelper() {
        TestOp = new TestOp();
        Options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        Options.TypeInfoResolverChain.Add(SourceGenerationContext.Default);
        SingleEfficiency = new Dictionary<string, Efficiency>();
        Results = new ConcurrentBag<(OpEnumData[] comb, Efficiency eff)>();
    }

    static void Clear() {
        SingleEfficiency.Clear();
        Results.Clear();
    }
    static Simulator Proc(OpEnumData[] comb, ParallelLoopState state, Simulator simu) {
        var eff = TestMany(simu, comb);
        var manu_eff = eff.ManuEff;
        var trad_eff = eff.TradEff;
        var power_eff = eff.PowerEff;
        foreach (var data in comb) {
            var single_eff = SingleEfficiency[data.Name];
            manu_eff += single_eff.ManuEff;
            trad_eff += single_eff.TradEff;
            power_eff += single_eff.PowerEff;
        }
        if (manu_eff < 0 || trad_eff < 0 || power_eff < 0) {
            return simu;
        }
        if (manu_eff == 0 && trad_eff == 0 && power_eff == 0) {
            return simu;
        }
        Results.Add((comb, new Efficiency(manu_eff, trad_eff, power_eff)));
        return simu;
    }
    public static List<(OpEnumData[] comb, Efficiency eff)> Enumerate(JsonDocument json) {
        Clear();
        var result = EnumerateImpl(json).ToList();
        Clear();
        return result;
    }
    public static void Enumerate(JsonDocument json, Utf8JsonWriter writer) {
        Clear();
        var result = EnumerateImpl(json);
        writer.WriteStartArray();
        foreach (var (comb, eff) in result) {
            writer.WriteStartObject();
              writer.WritePropertyName("comb");
              writer.WriteRawValue(JsonSerializer.Serialize(comb, Options));
              //writer.WriteStartArray();
              //  foreach (var data in comb) {
              //      writer.WriteStartObject();
              //      writer.WriteString("name", data.Name);
              //      writer.WriteEndObject();
              //  }
              //writer.WriteEndArray();

              writer.WritePropertyName("eff");
              writer.WriteStartObject();
                writer.WriteNumber("manu_eff", eff.ManuEff);
                writer.WriteNumber("trad_eff", eff.TradEff);
                writer.WriteNumber("power_eff", eff.PowerEff);
              writer.WriteEndObject();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.Flush();
        Clear();
        return;
    }
    static IOrderedEnumerable<(OpEnumData[] comb, Efficiency eff)> EnumerateImpl(JsonDocument json) {
        var root = json.RootElement;
        var preset = root.GetProperty("preset");
        var simu = InitSimulator(preset);
        Baseline = simu.GetEfficiency();

        var ops = root.GetProperty("ops").Deserialize<OpEnumData[]>(Options);
        foreach (var data in ops) {
            SingleEfficiency[data.Name] = TestSingle(simu, data);
        }
        for (int i = 1; i <= ops.Length; i++) {
            var combs = new Combination<OpEnumData>(ops, i);
            var enumerable = combs
                .ToEnumerable()
                .Where(comb => comb.DistinctBy(op => op.Name).Count() == i)
                .Select(comb => comb.ToArray());
            Parallel.ForEach(enumerable, () => InitSimulator(preset), Proc, simu => { });
        }

        return Results.OrderByDescending((v) => v.eff.GetScore() / v.comb.Length);
    }
    static Simulator InitSimulator(JsonElement elem) {
        var simu = new Simulator();
        foreach (var prop in elem.EnumerateObject()) {
            simu.SetFacilityState(prop.Name, prop.Value);
        }
        simu.FillTestOp();
        return simu;
    }
    static Efficiency GetEfficiency(this Simulator simu) {
        simu.Resolve();
        return new Efficiency(
            simu.TradingEfficiency,
            simu.ManufacturingEfficiency,
            simu.DronesEfficiency);
    }
    static void FillTestOp(this Simulator simu) {
        foreach (var fac in simu.Facilities) {
            fac?.FillTestOp();
        }
    }
    static void FillTestOp(this FacilityBase fac) {
        for (int i = 0; i < fac.AcceptOperatorNums; i++) {
            if (fac._operators[i]?.Name != "测试干员") {
                fac.RemoveAt(i);
                fac.AssignAt(TestOp.Clone(), i);
            }
        }
    }
    static bool AnyOp(this FacilityBase fac) {
        for (int i = 0; i < fac.AcceptOperatorNums; i++) {
            if (fac._operators[i]?.Name != "测试干员") {
                return true;
            }
        }
        return false;
    }
    static bool TestAssign(this FacilityBase fac, OperatorBase op) {
        for (int i = 0; i < fac.AcceptOperatorNums; i++) {
            var opInFac = fac._operators[i];
            if (opInFac == null || opInFac.Name == "测试干员") {
                fac.RemoveAt(i);
                fac.AssignAt(op, i);
                return true;
            }
        }
        return false;
    }
    static void Assign(this Simulator simu, OpEnumData data) {
        var op = simu.GetOperator(data.Name);
        var facName = data.Fac.ToLower();
        FacilityBase? fac = facName switch {
            "控制中枢" => simu.ControlCenter,
            "办公室" => simu.Office,
            "controlcenter" => simu.ControlCenter,
            "control center" => simu.ControlCenter,
            "reception" => simu.Reception,
            "crafting" => simu.Crafting,
            "office" => simu.Office,
            "training" => simu.Training,
            _ => null
        };
        if (fac != null) {
            var success = fac.TestAssign(simu.GetOperator(data.Name));
            if (!success) throw new Exception("没有足够的位置摆放干员");
            return;
        }
        if (facName == "trading" || facName == "贸易站") {
            TradingStation.OrderStrategy? strategy = data.Strategy switch {
                "赤金" => TradingStation.OrderStrategy.Gold,
                "龙门币" => TradingStation.OrderStrategy.Gold,
                "gold" => TradingStation.OrderStrategy.Gold,
                "lmb" => TradingStation.OrderStrategy.Gold,
                "源石" => TradingStation.OrderStrategy.OriginStone,
                "合成玉" => TradingStation.OrderStrategy.OriginStone,
                "originium" => TradingStation.OrderStrategy.OriginStone,
                _ => null
            };
            foreach (var trading in simu.TradingStations) {
                if (strategy != null) {
                    if (!trading.AnyOp()) {
                        trading.TestAssign(simu.GetOperator(data.Name));
                        trading.Strategy = strategy.Value;
                        return;
                    } else if (trading.Strategy == strategy) {
                        var success = trading.TestAssign(simu.GetOperator(data.Name));
                        if (success) return;
                    }
                } else {
                    var success = trading.TestAssign(simu.GetOperator(data.Name));
                    if (success) return;
                }
            }
        }
        if (facName == "manufacturing" || facName == "制造站") {
            Product? product = null;
            if (data.Product != null) {
                product = Product.AllProducts.Where(p => p.Name == data.Product).FirstOrDefault()
                    ?? throw new ApplicationException($"未知的产品名称 {product}");
            }
            foreach (var manufacturing in simu.ManufacturingStations) {
                if (product != null) {
                    if (!manufacturing.AnyOp()) {
                        manufacturing.TestAssign(op);
                        manufacturing.ChangeProduct(product);
                        return;
                    } else if (manufacturing.Product == product) {
                        var success = manufacturing.TestAssign(op);
                        if (success) return;
                    }
                } else {
                    var success = manufacturing.TestAssign(op);
                    if (success) return;
                }
            }
        }
        if (facName == "power" || facName == "发电站") {
            foreach (var power in simu.PowerStations) {
                var success = power.TestAssign(op);
                if (success) return;
            }
        }
        throw new Exception("未识别的设施或没有足够位置拜访干员");
    }
    static Efficiency TestSingle(Simulator simu, OpEnumData data) {
        simu.Assign(data);
        var diff = simu.GetEfficiency() - Baseline;
        var op = simu.GetOperator(data.Name);
        op.Facility?.FillTestOp();
        return diff;
    }
    static Efficiency TestMany(Simulator simu, IEnumerable<OpEnumData> datas) {
        foreach (var data in datas) {
            simu.Assign(data);
        }
        var diff = simu.GetEfficiency() - Baseline;
        simu.FillTestOp();
        return diff;
    }
}
