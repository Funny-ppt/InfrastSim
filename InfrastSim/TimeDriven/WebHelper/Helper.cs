using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace InfrastSim.TimeDriven.WebHelper;
public static partial class Helper {
    public static FacilityBase? GetFacilityByIndex(this Simulator simu, int index) {
        return simu.Facilities[index];
    }
    public static void SetUpgraded(this Simulator simu, string name, int upgraded) {
        if (simu.Operators.TryGetValue(name, out var value)) {
            value.Upgraded = upgraded;
        } else {
            throw new KeyNotFoundException($"未知的干员名称 {name}");
        }
    }
    public static string? GetVipName(this Simulator simu, int dormIndex) {
        return ((Dormitory?)simu.Dormitories[dormIndex])?.Vip?.Name;
    }

    /// <summary>
    /// 返回门牌号对应与simu.Facilities中的下标
    /// </summary>
    static int LabelToIndex(string label) => (label[1] - '0' - 1) * 3 + (label[3] - '0' - 1) + 9;
    static Regex _roomLabelRegex = RoomLabelRegex();
    static Regex _roomNameRegex = RoomNameWithOptionalIndexRegex();
    static FacilityBase? GetFacilityByName(this Simulator simu, string fac) {
        fac = fac.Replace('-', ' ').Replace('_', ' ').ToLower();
        if (_roomLabelRegex.IsMatch(fac)) {
            var index = LabelToIndex(fac);
            return simu.Facilities[index];
        }
        var match = _roomNameRegex.Match(fac);
        var fac_name = match.Groups[1].Value;
        if (fac_name == "dormitory") {
            var index = int.Parse(match.Groups[2].Value.Trim()) - 1;
            return simu.Dormitories[index];
        } else {
            return fac_name switch {
                "controlcenter" => simu.ControlCenter,
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

    public static TradingStation.OrderStrategy ToStrategy(string text) {
        return text switch {
            "龙门商法" => TradingStation.OrderStrategy.Gold,
            "赤金" => TradingStation.OrderStrategy.Gold,
            "龙门币" => TradingStation.OrderStrategy.Gold,
            "gold" => TradingStation.OrderStrategy.Gold,
            "lmb" => TradingStation.OrderStrategy.Gold,
            "开采协力" => TradingStation.OrderStrategy.OriginStone,
            "源石" => TradingStation.OrderStrategy.OriginStone,
            "合成玉" => TradingStation.OrderStrategy.OriginStone,
            "originium" => TradingStation.OrderStrategy.OriginStone,
            "originstone" => TradingStation.OrderStrategy.OriginStone,
            _ => throw new ApplicationException($"未知的订单类型 {text}")
        };
    }

    public static void SetFacilityState(this Simulator simu, string fac, JsonElement elem) {
        var facility = simu.GetFacilityByName(fac);
        if (facility != null) {
            if (elem.TryGetProperty("destroy", out _)) {
                if (_roomLabelRegex.IsMatch(fac)) {
                    var index = LabelToIndex(fac);
                    facility.RemoveAll();
                    simu.Facilities[index] = null;
                    return;
                }
            }
            if (elem.TryGetProperty("refactor", out var refactor)) {
                if (_roomLabelRegex.IsMatch(fac)) {
                    var index = LabelToIndex(fac);
                    FacilityBase new_fac = refactor.GetString() switch {
                        "Trading" => new TradingStation(),
                        "Manufacturing" => new ManufacturingStation(),
                        "Power" => new PowerStation(),
                        _ => throw new ArgumentException("未知或不受支持的设施类型")
                    }; 
                    simu.Facilities[index]?.RemoveAll();
                    simu.Facilities[index] = new_fac;
                }
            }
            if (elem.TryGetProperty("level", out var level)) {
                facility.SetLevel(level.GetInt32());
            }
            if (elem.TryGetProperty("strategy", out var strategy)) {
                if (facility is TradingStation trading) {
                    var strategyText = strategy.GetString().ToLower();
                    trading.Strategy = ToStrategy(strategyText);
                }
            }
            if (elem.TryGetProperty("product", out var prod)) {
                if (facility is ManufacturingStation manufacturing) {
                    var product = prod.GetString();
                    Product newProduct;
                    if (product.StartsWith("源石碎片")) {
                        var splits = product.Split('_', StringSplitOptions.RemoveEmptyEntries);
                        var consumes = splits[1];
                        newProduct = Product.AllProducts
                            .Where(p => p.Name == "源石碎片" && p.Consumes[1].Name == consumes)
                            .FirstOrDefault() ?? throw new ApplicationException($"未知的源石碎片材料 {consumes}");
                    }
                    else {
                        newProduct = Product.AllProducts
                            .Where(p => p.Name == product)
                            .FirstOrDefault() ?? throw new ApplicationException($"未知的产品名称 {product}");
                    }
                    Collect(simu, manufacturing);
                    manufacturing.ChangeProduct(newProduct);
                }
            }
            if (elem.TryGetProperty("operators", out var ops)) {
                var operators = ops.EnumerateArray().Select(e => simu.GetOperator(e.GetString()));
                facility.AssignMany(operators);
            }
            if (elem.TryGetProperty("drone", out var drone)) {
                (facility as IApplyDrones ?? throw new ArgumentException($"{fac} 未建造或不是可以使用无人机的设施"))
                    .ApplyDrones(simu, drone.GetInt32());
            }
            if (elem.TryGetProperty("collect", out var collect_idx)) {
                Collect(simu, facility, collect_idx.GetInt32());
            }
        } else {
            if (_roomLabelRegex.IsMatch(fac)) {
                var index = LabelToIndex(fac);
                facility = FacilityBase.FromJson(elem, simu);
                simu.Facilities[index] = facility;
            } else {
                fac = fac.Replace('-', ' ').Replace('_', ' ').ToLower();
                var match = _roomNameRegex.Match(fac);
                var fac_name = match.Groups[1].Value;
                if (fac_name == "dormitory") {
                    var index = int.Parse(match.Groups[2].Value.Trim()) - 1;
                    simu.Facilities[index + 5] = FacilityBase.FromJson(elem, simu);
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
                if (facility is ManufacturingStation manufacturing) {
                    var product = prod.GetString();
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