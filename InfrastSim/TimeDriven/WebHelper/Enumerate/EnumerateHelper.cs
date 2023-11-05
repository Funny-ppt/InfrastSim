using System.Collections.Concurrent;
using System.Text.Json;

namespace InfrastSim.TimeDriven.WebHelper;
public static class EnumerateHelper {
    static readonly OperatorBase TestOp;
    internal static readonly JsonSerializerOptions Options;

    static EnumerateHelper() {
        TestOp = new TestOp();
        Options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        Options.TypeInfoResolverChain.Add(SourceGenerationContext.Default);
    }

    public static void Enumerate(JsonDocument json, Utf8JsonWriter writer) {
        var result = new EnumerateContext().Enumerate(json);
        writer.WriteStartArray();
        foreach (var (comb, eff, extra_eff) in result) {
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
              writer.WritePropertyName("extra_eff");
              writer.WriteStartObject();
                writer.WriteNumber("manu_eff", extra_eff.ManuEff);
                writer.WriteNumber("trad_eff", extra_eff.TradEff);
                writer.WriteNumber("power_eff", extra_eff.PowerEff);
              writer.WriteEndObject();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.Flush();
        return;
    }

    internal static Efficiency GetEfficiency(this Simulator simu) {
        simu.Resolve();
        return new Efficiency(
            simu.TradingEfficiency,
            simu.ManufacturingEfficiency,
            simu.DronesEfficiency);
    }
    internal static void FillTestOp(this Simulator simu) {
        foreach (var fac in simu.Facilities) {
            fac?.FillTestOp();
        }
    }
    internal static void FillTestOp(this FacilityBase fac) {
        for (int i = 0; i < fac.AcceptOperatorNums; i++) {
            if (fac._operators[i]?.Name != "测试干员") {
                fac.RemoveAt(i);
                fac.AssignAt(TestOp.Clone(), i);
            }
        }
    }
    internal static bool AnyOp(this FacilityBase fac) {
        for (int i = 0; i < fac.AcceptOperatorNums; i++) {
            if (fac._operators[i]?.Name != "测试干员") {
                return true;
            }
        }
        return false;
    }
    internal static bool TestAssign(this FacilityBase fac, OperatorBase op) {
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
    internal static void Assign(this Simulator simu, OpEnumData data) {
        var op = simu.GetOperator(data.Name);
        var facName = data.Fac.ToLower();
        FacilityBase? fac = facName switch {
            "控制中枢" => simu.ControlCenter,
            "controlcenter" => simu.ControlCenter,
            "control center" => simu.ControlCenter,
            "reception" => simu.Reception,
            "crafting" => simu.Crafting,
            "办公室" => simu.Office,
            "office" => simu.Office,
            "training" => simu.Training,
            _ => null
        };
        if (fac != null) {
            if (!fac.TestAssign(simu.GetOperator(data.Name)))
                throw new Exception("没有足够的位置摆放干员");
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
                        trading.TestAssign(op);
                        trading.Strategy = strategy.Value;
                        return;
                    } else if (trading.Strategy == strategy) {
                        if (trading.TestAssign(op)) return;
                    }
                } else {
                    if (trading.TestAssign(op)) return;
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
                        if (manufacturing.TestAssign(op)) return;
                    }
                } else {
                    if (manufacturing.TestAssign(op)) return;
                }
            }
        }
        if (facName == "power" || facName == "发电站") {
            foreach (var power in simu.PowerStations) {
                if (power.TestAssign(op)) return;
            }
        }
        if (facName.StartsWith("dorm") || facName == "宿舍") {
            foreach (var dorm in simu.Dormitories) {
                if (dorm?.TestAssign(op) ?? false) return;
            }
        }
        throw new Exception("未识别的设施或没有足够位置拜访干员");
    }
}
