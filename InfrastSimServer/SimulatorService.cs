using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InfrastSimServer; 
public class SimulatorService {
    //public static readonly SimulatorService Instance = new(); 

    private int _simuId = 0;
    private ConcurrentDictionary<int, Simulator> _simus = new();

    public void Create(HttpContext httpContext) {
        var id = Interlocked.Increment(ref _simuId);
        var simu = _simus[_simuId] = new Simulator();

        httpContext.Response.ContentType = "application/json";
        using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        writer.WriteStartObject();
        writer.WriteNumber("id", id);
        writer.WriteItem("data", simu);
        writer.WriteEndObject();
        writer.Flush();
    }

    public void CreateWithData(HttpContext httpContext) {
        var doc = JsonDocument.Parse(httpContext.Request.Body);
        var id = Interlocked.Increment(ref _simuId);
        var simu = _simus[_simuId] = new Simulator(doc.RootElement);

        httpContext.Response.ContentType = "application/json";
        using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        writer.WriteStartObject();
        writer.WriteNumber("id", id);
        writer.WriteItem("data", simu);
        writer.WriteEndObject();
        writer.Flush();
    }

    public void GetData(HttpContext httpContext, int id, bool detailed = false) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.Resolve();
        httpContext.Response.ContentType = "application/json";
        using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        writer.WriteItemValue(simu, detailed);
    }

    public void Simulate(HttpContext httpContext, int id) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.Simulate(TimeSpan.FromMinutes(1));
        httpContext.Response.ContentType = "application/json";
    }

    public async Task SetFacilityState(HttpContext httpContext, int id, string facility) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var doc = await JsonDocument.ParseAsync(httpContext.Request.Body);
        simu.SetFacilityState(facility, doc.RootElement);
    }

    public void GetOperators(HttpContext httpContext, int id) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }


        httpContext.Response.ContentType = "application/json";
        using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        writer.WriteOperators(simu);
        writer.Flush();
    }

    public void SetUpgraded(HttpContext httpContext, int id, Dictionary<string, int> ops) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        foreach (var kvp in ops) {
            simu.SetUpgraded(kvp.Key, kvp.Value);
        }
    }

    public void SelectOperators(HttpContext httpContext, int id, SelectOperatorsData data) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.SelectOperators(data.Facility, data.Operators);
    }

    public void RemoveOperator(HttpContext httpContext, int id, string facility, int idx) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.RemoveOperator(facility, idx);
    }

    public void RemoveOperators(HttpContext httpContext, int id, string facility) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.RemoveOperators(facility);
    }

    public void CollectAll(HttpContext httpContext, int id) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.CollectAll();
    }

    public void Collect(HttpContext httpContext, int id, string facility, int idx = 0) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.Collect(facility, idx);
    }

    public void UseDrones(HttpContext httpContext, int id, string facility, int amount) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.UseDrones(facility, amount);
    }

    public void Sanity(HttpContext httpContext, int id, int amount) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.AddDrones(amount);
    }

    public void SimulateUntil(HttpContext httpContext, int id, DateTime until) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.SimulateUntil(until, TimeSpan.FromMinutes(1));
        httpContext.Response.ContentType = "application/json";
    }

    public async Task GetDataForMower(HttpContext httpContext, int id) {
        if (!_simus.TryGetValue(id, out var simu)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, true);
        writer.Flush();
        ms.Position = 0;
        var node = await JsonNode.ParseAsync(ms);
        node!["power_limit"] = simu.TotalPowerProduce;
        node["power_usage"] = simu.TotalPowerConsume;
        node["drone_limit"] = 200;
        node["drone_count"] = simu.Drones;
        node["next_drone"] = (int) simu.NextDroneTimeInSeconds;
        var globalProperties = new JsonObject();
        foreach (var prop in simu.GlobalValues) {
            globalProperties.Add(prop.Item1, (int)prop.Item2);
        }
        node["global_properties"] = globalProperties;
        var facilities = new JsonObject();
        facilities["Control Center"] = RewriteFacility(node["control-center"]!);
        facilities["Office"] = RewriteFacility(node["office"]!);
        facilities["Recpetion Room"] = RewriteFacility(node["reception"]!);
        facilities["training"] = RewriteFacility(node["training"]!);
        facilities["crafting"] = RewriteFacility(node["crafting"]!);
        for (int i = 0; i < 4; i++) {
            var fac = node["dormitories"]![i];
            if (fac != null) {
                var label = $"Dormitory {i + 1}";
                facilities[label] = RewriteDormitory(fac, simu, i + 1);
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


        httpContext.Response.ContentType = "application/json";
        using var respWriter = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        node.WriteTo(respWriter);
        writer.Flush();
    }

    static JsonObject RewriteOrder(JsonNode order, int timeRemain = 0) {
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
        newOrder["time_remain"] = timeRemain;
        return newOrder;
    }
    static JsonObject RewriteOperator(JsonNode op) {
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
    static JsonObject RewriteFacility(JsonNode fac) {
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
    static JsonObject RewriteDormitory(JsonNode fac, Simulator simu, int id) {
        var newfac = RewriteFacility(fac);
        newfac["name"] = $"宿舍{id}";
        newfac["vip"] = simu.GetVipName(id);
        return newfac;
    }
    static JsonObject RewriteManufacturing(JsonNode fac, Simulator simu) {
        var newfac = RewriteFacility(fac);

        newfac["name"] = "制造站";
        newfac["base_capacity"] = fac["base-capacity"]!.DeepClone();
        newfac["capacity"] = fac["capacity"]!.DeepClone();
        newfac["total_efficiency"] =
                fac["base-efficiency"]!.GetValue<double>()
              + fac["operators-efficiency"]!.GetValue<double>()
              + simu.GlobalManufacturingEffiency;
        newfac["finished_product"] = fac["product-count"]!.DeepClone();
        newfac["product"] = fac["product"]!.DeepClone();
        newfac["progress"] = fac["progress"]!.DeepClone();

        return newfac;
    }
    static JsonObject RewriteTrading(JsonNode fac, Simulator simu) {
        var orders = new JsonArray();
        foreach (var order in fac["orders"]!.AsArray()) {
            orders.Add(RewriteOrder(order!));
        }
        orders.Add(RewriteOrder(fac["current-order"]!, fac["remains"]!.GetValue<int>()));

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
    static JsonObject RewritePower(JsonNode fac) {
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
