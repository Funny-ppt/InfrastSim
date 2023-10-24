using System.Text.Json.Nodes;

namespace InfrastSim.TimeDriven.WebHelper;
public class MowerHelper {
    public static JsonNode RewriteSimulator(Simulator simu, JsonNode node) {
        node!["power_limit"] = simu.TotalPowerProduce;
        node["power_usage"] = simu.TotalPowerConsume;
        node["drone_limit"] = 200;
        node["drone_count"] = simu.Drones;
        node["next_drone"] = (int)simu.NextDroneTimeInSeconds;
        var globalProperties = new JsonObject();
        foreach (var prop in simu.GlobalValues) {
            globalProperties.Add(prop.Item1, (int)prop.Item2);
        }
        node["global_properties"] = globalProperties;
        var facilities = new JsonObject();
        facilities["controlCenter"] = RewriteFacility(node["control-center"]!);
        facilities["office"] = RewriteFacility(node["office"]!);
        facilities["recpetionRoom"] = RewriteFacility(node["reception"]!);
        facilities["training"] = RewriteFacility(node["training"]!);
        facilities["crafting"] = RewriteFacility(node["crafting"]!);
        for (int i = 0; i < 4; i++) {
            var fac = node["dormitories"]![i];
            if (fac != null) {
                var label = $"Dormitory {i + 1}";
                facilities[label] = RewriteDormitory(fac, simu, i);
            }
        }
        for (int i = 0; i < 9; i++) {
            var fac = node["modifiable-facilities"]![i];
            if (fac != null) {
                var label = $"B{i / 3 + 1}0{i % 3 + 1}";
                JsonObject? newfac = null;
                switch (fac["type"]!.GetValue<string>()) {
                    case "Trading":
                        newfac = RewriteTrading(fac, simu);
                        break;
                    case "Manufacturing":
                        newfac = RewriteManufacturing(fac, simu);
                        break;
                    case "Power":
                        newfac = RewritePower(fac);
                        break;
                    default:
                        break;
                }
                facilities[label] = newfac;
            }
        }
        node["facilities"] = facilities;
        var operators = new JsonArray();
        foreach (var op in node["operators"]!.AsArray()) {
            operators.Add(RewriteOperator(op));
        }
        node["operators-mower"] = operators;
        return node;
    }
    public static JsonObject RewriteOrder(JsonNode order, double timeRemain = 0) {
        var newOrder = new JsonObject();
        switch (order["consume"]!.GetValue<string>()) {
            case "赤金":
                newOrder["type"] = "gold";
                newOrder["gold"] = order["consume-count"]!.DeepClone();
                newOrder["lmb"] = order["earn-count"]!.DeepClone();
                break;
            case "源石碎片":
                newOrder["type"] = "originium";
                newOrder["originium"] = order["consume-count"]!.DeepClone();
                newOrder["orundum"] = order["earn-count"]!.DeepClone();
                break;
        }
        newOrder["time_total"] = new TimeSpan(order["produce-time"]!.GetValue<long>()).TotalSeconds;
        newOrder["time_remain"] = (long)timeRemain;
        return newOrder;
    }
    public static JsonObject RewriteOperator(JsonNode op) {
        var newop = new JsonObject {
            ["name"] = op["name"]!.DeepClone(),
            ["morale"] = op["mood"]!.DeepClone(),
            ["efficiency"] = op["efficiency"]!.DeepClone()
        };

        if (op["mood"]!.GetValue<double>() > 1e-9) {
            newop["time"] = op["working-time-seconds"]!.DeepClone();
        } else {
            newop["time"] = 0;
        }
        var moraleConsumeRate = op["mood-consume-rate"]!.GetValue<double>();
        if (moraleConsumeRate >= 0) {
            newop["morale_consume"] = moraleConsumeRate;
            newop["morale_increase"] = 0;
        } else {
            newop["morale_consume"] = 0;
            newop["morale_increase"] = -moraleConsumeRate;
        }

        return newop;
    }
    public static JsonObject RewriteFacility(JsonNode fac) {
        var ops = new JsonArray();
        foreach (var op in fac["operators"]!.AsArray()) {
            ops.Add(RewriteOperator(op!));
        }

        var newfac = new JsonObject {
            ["level"] = fac["level"]!.DeepClone(),
            ["base_efficiency"] = fac["base-efficiency"]!.DeepClone(),
            ["operators_efficiency"] = fac["operators-efficiency"]!.DeepClone(),
            ["operators"] = ops,
        };

        return newfac;
    }
    public static JsonObject RewriteDormitory(JsonNode fac, Simulator simu, int id) {
        var newfac = RewriteFacility(fac);
        newfac["name"] = $"宿舍{id + 1}";
        newfac["vip"] = simu.GetVipName(id);
        return newfac;
    }
    public static JsonObject RewriteManufacturing(JsonNode fac, Simulator simu) {
        var newfac = RewriteFacility(fac);

        newfac["name"] = "制造站";
        newfac["base_capacity"] = fac["base-capacity"]!.DeepClone();
        newfac["capacity"] = fac["capacity"]!.DeepClone();
        newfac["total_efficiency"] =
                fac["base-efficiency"]!.GetValue<double>()
              + fac["operators-efficiency"]!.GetValue<double>()
              + simu.GlobalManufacturingEffiency;
        newfac["finished_product"] = fac["product-count"]!.DeepClone();
        newfac["product"] = fac["product"]?.DeepClone() ?? null;
        newfac["progress"] = fac["progress"]!.DeepClone();

        return newfac;
    }
    public static JsonObject RewriteTrading(JsonNode fac, Simulator simu) {
        var orders = new JsonArray();
        foreach (var order in fac["orders"]!.AsArray()) {
            orders.Add(RewriteOrder(order!));
        }
        var cur_order = fac["current-order"];
        if (cur_order != null) orders.Add(RewriteOrder(cur_order, fac["remains"]!.GetValue<double>()));

        var newfac = RewriteFacility(fac);
        newfac["name"] = "贸易站";
        newfac["base_order_limit"] = fac["base-capacity"]!.DeepClone();
        newfac["order_limit"] = fac["capacity"]!.DeepClone();
        newfac["base_efficiency"] = fac["base-efficiency"]!.DeepClone();
        newfac["operators_efficiency"] = fac["operators-efficiency"]!.DeepClone();
        newfac["total_efficiency"] =
                fac["base-efficiency"]!.GetValue<double>()
              + fac["operators-efficiency"]!.GetValue<double>()
              + simu.GlobalTradingEffiency;
        newfac["order_chance"] = fac["order-chance"]!.DeepClone();
        newfac["orders"] = orders;

        return newfac;
    }
    public static JsonObject RewritePower(JsonNode fac) {
        var ops = new JsonArray();
        foreach (var op in fac["operators"]!.AsArray()) {
            ops.Add(RewriteOperator(op!));
        }
        var base_eff = fac["base-efficiency"]!.GetValue<double>() - 1;
        var newfac = new JsonObject {
            ["name"] = "发电站",
            ["base_order_limit"] = fac["base-capacity"]!.DeepClone(),
            ["order_limit"] = fac["capacity"]!.DeepClone(),
            ["base_drone_efficiency"] = base_eff,
            ["drone_efficiency"] = base_eff + fac["operators-efficiency"]!.GetValue<double>(),
            ["order_chance"] = fac["order-chance"]!.DeepClone(),
            ["operators"] = ops
        };
        return newfac;
    }
}
