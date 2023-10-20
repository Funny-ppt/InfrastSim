using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InfrastSim.TimeDriven.WebHelper;
public static partial class Helper {
    public static void SetUpgraded(this Simulator simu, string name, int upgraded) {
        if (simu.Operators.TryGetValue(name, out var value)) {
            value.Upgraded = upgraded;
        }
    }
    public static string? GetVipName(this Simulator simu, int dormIndex) {
        return simu.Dormitories[dormIndex]?.GetVip()?.Name;
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
            return index < 4 ? simu.Dormitories[index] : null;
        } else {
            return fac_name switch {
                "control center" => simu.ControlCenter,
                "reception" => simu.Reception,
                "crafting" => simu.Crafting,
                "office" => simu.Office,
                "training" => simu.Training,
                _ => null
            };
        }
    }
    public static void SelectOperators(this Simulator simu, string fac, string[] ops) {
        var facility = GetFacilityByName(simu, fac);
        if (facility != null) {
            foreach (var op in ops) {
                facility.Assign(simu.GetOperator(op));
            }
        }
    }
    public static void RemoveOperator(this Simulator simu, string fac, int idx) {
        var facility = GetFacilityByName(simu, fac);
        facility?.Remove(facility.Operators.Skip(idx - 1).FirstOrDefault());
    }
    public static void RemoveOperators(this Simulator simu, string fac) {
        var facility = GetFacilityByName(simu, fac);
        facility?.RemoveAll();
    }
    public static void UseDrones(this Simulator simu, string fac, int amount) {
        if (GetFacilityByName(simu, fac) is IApplyDrones facility) {
            facility.ApplyDrones(simu, amount);
        }
    }
    static void Collect(Simulator simu, FacilityBase? fac, int idx = 0) {
        if (fac is ManufacturingStation manufacturing && manufacturing.Product != null) {
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
        }
    }
    public static void Collect(this Simulator simu, string fac, int idx) {
        Collect(simu, simu.GetFacilityByName(fac), idx);
    }
    public static void CollectAll(this Simulator simu) {
        foreach (var fac in simu.ModifiableFacilities) {
            Collect(simu, fac);
        }
    }

    public static void SetFacilityState(this Simulator simu, string fac, JsonElement elem) {
        var facility = simu.GetFacilityByName(fac);
        if (facility != null) {
            if (elem.TryGetProperty("level", out var level)) {
                facility.SetLevel(level.GetInt32());
            }
            if (elem.TryGetProperty("strategy", out var strategy)) {
                if (facility is TradingStation trading) {
                    trading.Strategy = strategy.GetString().ToLower() switch {
                        "gold" => TradingStation.OrderStrategy.Gold,
                        "originium" => TradingStation.OrderStrategy.OriginStone,
                        _ => TradingStation.OrderStrategy.Gold,
                    };
                }
            }
            if (elem.TryGetProperty("product", out var prod)) {
                var product = prod.GetString();
                if (facility is ManufacturingStation manufacturing) {
                    var newProduct = Product.AllProducts.Where(p => p.Name == product).FirstOrDefault();
                    if (newProduct != null) {
                        Collect(simu, manufacturing);
                        manufacturing.ChangeProduct(newProduct);
                    }
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
                    facility.Assign(simu.GetOperator(opName));
                }
            }
            if (elem.TryGetProperty("operators-force-replace", out var ops2)) {
                var operators = ops2.EnumerateArray().Select(e => simu.GetOperator(e.GetString()));
                facility.AssignMany(operators.Where(op => op != null));
            }
            if (elem.TryGetProperty("drone", out var drone)) {
                (facility as IApplyDrones)?.ApplyDrones(simu, drone.GetInt32());
            }
        } else {
            if (_roomLabelRegex.IsMatch(fac)) {
                var index = LabelToIndex(fac);
                facility = FacilityBase.FromJson(elem, simu);
                simu.ModifiableFacilities[index] = facility;
                simu.AllFacilities[index + 9] = facility;
            } else {
                fac = fac.Replace('-', ' ').Replace('_', ' ').ToLower();
                var match = _roomNameRegex.Match(fac);
                var fac_name = match.Groups[1].Value;
                if (fac_name == "dormitory") {
                    var index = int.Parse(match.Groups[2].Value.Trim()) - 1;
                    simu.Dormitories[index] = FacilityBase.FromJson(elem, simu) as Dormitory;
                }
            }

            if (elem.TryGetProperty("strategy", out var strategy)) {
                if (facility is TradingStation trading) {
                    trading.Strategy = strategy.GetString().ToLower() switch {
                        "gold" => TradingStation.OrderStrategy.Gold,
                        "originium" => TradingStation.OrderStrategy.OriginStone,
                        _ => TradingStation.OrderStrategy.Gold,
                    };
                }
            }
            if (elem.TryGetProperty("product", out var prod)) {
                var product = prod.GetString();
                if (facility is ManufacturingStation manufacturing) {
                    var newProduct = Product.AllProducts.Where(p => p.Name == product).FirstOrDefault();
                    if (newProduct != null) {
                        manufacturing.ChangeProduct(newProduct);
                    }
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

    [GeneratedRegex(@"^([a-zA-Z ]+)( \d)?$")]
    private static partial Regex RoomNameWithOptionalIndexRegex();
}