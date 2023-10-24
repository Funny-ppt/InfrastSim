using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace InfrastSim.TimeDriven.WebHelper;
public static partial class Helper {
    public static void SetUpgraded(this Simulator simu, string name, int upgraded) {
        if (simu.Operators.TryGetValue(name, out var value)) {
            value.Upgraded = upgraded;
        } else {
            throw new KeyNotFoundException($"未知的干员名称 {name}");
        }
    }
    public static string? GetVipName(this Simulator simu, int dormIndex) {
        return ((Dormitory?)simu.Dormitories[dormIndex])?.GetVip()?.Name;
    }

    static int LabelToIndex(string label) => (label[1] - '0' - 1) * 3 + label[3] - '0' - 1;
    static Regex _roomLabelRegex = RoomLabelRegex();
    static Regex _roomNameRegex = RoomNameWithOptionalIndexRegex();
    static FacilityBase? GetFacilityByName(this Simulator simu, string fac) {
        fac = fac.Replace('-', ' ').Replace('_', ' ').ToLower();
        if (_roomLabelRegex.IsMatch(fac)) {
            var index = LabelToIndex(fac);
            return simu.ModifiableFacilities[index];
        }
        var match = _roomNameRegex.Match(fac);
        var fac_name = match.Groups[1].Value;
        if (fac_name == "dormitory") {
            var index = int.Parse(match.Groups[2].Value.Trim()) - 1;
            return simu.Dormitories[index];
        } else {
            return fac_name switch {
                "control center" => simu.ControlCenter,
                "reception" => simu.Reception,
                "crafting" => simu.Crafting,
                "office" => simu.Office,
                "training" => simu.Training,
                _ => throw new ArgumentException($"{fac} 名称不存在对应的设施")
            };
        }
    }
    public static void SelectOperators(this Simulator simu, string fac, string[] ops) {
        var facility = GetFacilityByName(simu, fac)
            ?? throw new ArgumentException($"{fac} 名称对应的设施未建造");
        foreach (var op in ops) {
            facility.Assign(simu.GetOperator(op));
        }
    }
    public static void RemoveOperator(this Simulator simu, string fac, int idx) {
        var facility = GetFacilityByName(simu, fac)
            ?? throw new ArgumentException($"{fac} 名称对应的设施未建造");
        facility.Remove(facility.Operators.Skip(idx - 1).FirstOrDefault());
    }
    public static void RemoveOperators(this Simulator simu, string fac) {
        var facility = GetFacilityByName(simu, fac)
            ?? throw new ArgumentException($"{fac} 名称对应的设施未建造");
        facility.RemoveAll();
    }
    public static int UseDrones(this Simulator simu, string fac, int amount) {
        if (GetFacilityByName(simu, fac) is IApplyDrones facility) {
            return facility.ApplyDrones(simu, amount);
        } else {
            throw new ArgumentException($"{fac} 未建造或不是可以使用无人机的设施");
        }
    }
    static void Collect(Simulator simu, FacilityBase? fac, int idx = 0) {
        if (fac is ManufacturingStation manufacturing) {
            if (manufacturing.Product == null) return;

            var product = manufacturing.Product;
            simu.AddMaterial(product.Name, manufacturing.ProductCount);
            if (product.Consumes != null) {
                foreach (var mat in product.Consumes) {
                    simu.RemoveMaterial(mat, manufacturing.ProductCount);
                }
            }
            manufacturing.ProductCount = 0;
        } else if (fac is TradingStation trading) {
            if (idx == 0) {
                foreach (var order in trading.Orders) {
                    simu.AddMaterial(order.Earns);
                    simu.RemoveMaterial(order.Consumes);
                }
                trading.RemoveAllOrder();
            } else {
                var order = trading.Orders.Skip(idx - 1).FirstOrDefault();
                if (order != null) {
                    simu.AddMaterial(order.Earns);
                    simu.RemoveMaterial(order.Consumes);
                    trading.RemoveOrder(order);
                }
            }
        } else {
            throw new ArgumentException($"{fac} 名称对应的设施不是制造站或贸易站或未建造");
        }
    }
    public static void Collect(this Simulator simu, string fac, int idx) {
        Collect(simu, simu.GetFacilityByName(fac), idx);
    }
    public static void CollectAll(this Simulator simu) {
        foreach (var fac in simu.ModifiableFacilities) {
            try {
                Collect(simu, fac);
            } catch { }
        }
    }

    public static void SetFacilityState(this Simulator simu, string fac, JsonElement elem) {
        var facility = simu.GetFacilityByName(fac);
        if (facility != null) {
            if (elem.TryGetProperty("destroy", out var _)) {
                if (_roomLabelRegex.IsMatch(fac)) {
                    var index = LabelToIndex(fac);
                    facility.RemoveAll();
                    simu.AllFacilities[9 + index] = null;
                    return;
                }
            }
            if (elem.TryGetProperty("level", out var level)) {
                facility.SetLevel(level.GetInt32());
            }
            if (elem.TryGetProperty("strategy", out var strategy)) {
                if (facility is TradingStation trading) {
                    var strategyText = strategy.GetString().ToLower();
                    trading.Strategy = strategyText switch {
                        "gold" => TradingStation.OrderStrategy.Gold,
                        "originium" => TradingStation.OrderStrategy.OriginStone,
                        _ => throw new ApplicationException($"未知的订单类型 {strategyText}")
                    };
                }
            }
            if (elem.TryGetProperty("product", out var prod)) {
                var product = prod.GetString();
                if (facility is ManufacturingStation manufacturing) {
                    var newProduct = Product.AllProducts.Where(p => p.Name == product).FirstOrDefault()
                        ?? throw new ApplicationException($"未知的产品名称 {product}");
                    Collect(simu, manufacturing);
                    manufacturing.ChangeProduct(newProduct);
                }
            }
            if (elem.TryGetProperty("operators", out var ops)) {
                var opNames = ops.EnumerateArray().Select(e => e.GetString());
                foreach (var op in facility.Operators) {
                    if (!opNames.Contains(op.Name)) {
                        facility.Remove(op);
                    }
                }
                foreach (var opName in opNames) {
                    var op = simu.GetOperator(opName);
                    facility.Assign(op);
                }
            }
            if (elem.TryGetProperty("operators-force-replace", out var ops2)) {
                var operators = ops2.EnumerateArray().Select(e => simu.GetOperator(e.GetString()));
                facility.AssignMany(operators);
            }
            if (elem.TryGetProperty("drone", out var drone)) {
                (facility as IApplyDrones ?? throw new ArgumentException($"{fac} 未建造或不是可以使用无人机的设施"))
                    .ApplyDrones(simu, drone.GetInt32());
            }
        } else {
            if (_roomLabelRegex.IsMatch(fac)) {
                var index = LabelToIndex(fac);
                facility = FacilityBase.FromJson(elem, simu);
                simu.AllFacilities[index + 9] = facility;
            } else {
                fac = fac.Replace('-', ' ').Replace('_', ' ').ToLower();
                var match = _roomNameRegex.Match(fac);
                var fac_name = match.Groups[1].Value;
                if (fac_name == "dormitory") {
                    var index = int.Parse(match.Groups[2].Value.Trim()) - 1;
                    simu.AllFacilities[index + 5] = FacilityBase.FromJson(elem, simu);
                }
            }

            if (elem.TryGetProperty("strategy", out var strategy)) {
                if (facility is TradingStation trading) {
                    var strategyText = strategy.GetString().ToLower();
                    trading.Strategy = strategyText switch {
                        "gold" => TradingStation.OrderStrategy.Gold,
                        "originium" => TradingStation.OrderStrategy.OriginStone,
                        _ => throw new ApplicationException($"未知的订单类型 {strategyText}")
                    };
                }
            }
            if (elem.TryGetProperty("product", out var prod)) {
                var product = prod.GetString();
                if (facility is ManufacturingStation manufacturing) {
                    var newProduct = Product.AllProducts.Where(p => p.Name == product).FirstOrDefault()
                        ?? throw new ApplicationException($"未知的产品名称 {product}");
                    Collect(simu, manufacturing);
                    manufacturing.ChangeProduct(newProduct);
                }
            }
        }
    }

    public static void WriteOperators(this Utf8JsonWriter writer, Simulator simu) {
        writer.WriteStartArray();
        foreach (var op in simu.Operators.Values) {
            writer.WriteItemValue(op, true);
        }
        writer.WriteEndArray();
    }

    [GeneratedRegex(@"^[bB][1-3]0[1-3]$")]
    private static partial Regex RoomLabelRegex();

    [GeneratedRegex(@"^([a-zA-Z ]+)( [1-4])?$")]
    private static partial Regex RoomNameWithOptionalIndexRegex();
}