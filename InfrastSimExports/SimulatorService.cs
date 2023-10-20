using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InfrastSim.Exports;
public static class SimulatorService {
    private static int _simuId = 0;
    private static ConcurrentDictionary<int, Simulator> _simus = new();

    static Simulator GetSimulator(int id) {
        if (!_simus.TryGetValue(id, out var simulator)) {
            throw new KeyNotFoundException();
        }
        return simulator;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateSimulator")]
    public static int Create() {
        var id = Interlocked.Increment(ref _simuId);
        _simus[id] = new Simulator();
        return id;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateSimulatorWithData")]
    public static int CreateWithData(IntPtr pJson) {
        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);
        var id = Interlocked.Increment(ref _simuId);
        _simus[id] = new Simulator(doc.RootElement);
        return id;
    }

    [UnmanagedCallersOnly(EntryPoint = "DestroySimulator")]
    public static bool Destory(int id) {
        if (!_simus.ContainsKey(id)) {
            return false;
        }

        return _simus.TryRemove(id, out _);
    }

    [UnmanagedCallersOnly(EntryPoint = "GetData")]
    public static IntPtr GetData(int id, bool detailed = true) {
        var simu = GetSimulator(id);

        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, detailed);
        ms.WriteByte(0);

        var ptr = Marshal.AllocHGlobal((int)ms.Length);
        Marshal.Copy(ms.GetBuffer(), 0, ptr, (int)ms.Length);
        return ptr;
    }


    [UnmanagedCallersOnly(EntryPoint = "Simulate")]
    public static void Simulate(int id, int minutes, int seconds, int timespan) {
        if (!_simus.TryGetValue(id, out var simu)) {
            return;
        }
        
        var time = new TimeSpan(0, minutes, seconds);
        var span = TimeSpan.FromSeconds(timespan);
        simu.SimulateUntil(simu.Now + time, span);
    }


    [UnmanagedCallersOnly(EntryPoint = "SetFacilityState")]
    public static void SetFacilityState(int id, IntPtr pFacility, IntPtr pJson) {
        var simu = GetSimulator(id);
        var fac = Marshal.PtrToStringUTF8(pFacility) ?? string.Empty;
        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);
        simu.SetFacilityState(fac, doc.RootElement);
    }


    [UnmanagedCallersOnly(EntryPoint = "GetOperators")]
    public static IntPtr GetOperators(int id) {
        var simu = GetSimulator(id);
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteOperators(simu);
        writer.Flush();

        ms.WriteByte(0);

        var ptr = Marshal.AllocHGlobal((int)ms.Length);
        Marshal.Copy(ms.GetBuffer(), 0, ptr, (int)ms.Length);
        return ptr;
    }


    [UnmanagedCallersOnly(EntryPoint = "SetUpgraded")]
    public static void SetUpgraded(int id, IntPtr pJson) {
        var simu = GetSimulator(id);
        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);

        foreach (var prop in doc.RootElement.EnumerateObject()) {
            simu.SetUpgraded(prop.Name, prop.Value.GetInt32());
        }
    }


    [UnmanagedCallersOnly(EntryPoint = "RemoveOperator")]
    public static void RemoveOperator(int id, IntPtr pFacility, int idx) {
        var simu = GetSimulator(id);
        var facility = Marshal.PtrToStringUTF8(pFacility) ?? string.Empty;
        simu.RemoveOperator(facility, idx);
    }

    [UnmanagedCallersOnly(EntryPoint = "RemoveOperators")]
    public static void RemoveOperators(int id, IntPtr pFacility) {
        var simu = GetSimulator(id);
        var facility = Marshal.PtrToStringUTF8(pFacility) ?? string.Empty;
        simu.RemoveOperators(facility);
    }

    [UnmanagedCallersOnly(EntryPoint = "CollectAll")]
    public static void CollectAll(int id) {
        var simu = GetSimulator(id);
        simu.CollectAll();
    }

    [UnmanagedCallersOnly(EntryPoint = "Collect")]
    public static void Collect(int id, IntPtr pFacility, int idx = 0) {
        var simu = GetSimulator(id);
        var facility = Marshal.PtrToStringUTF8(pFacility) ?? string.Empty;
        simu.Collect(facility, idx);
    }


    [UnmanagedCallersOnly(EntryPoint = "UseDrones")]
    public static void UseDrones(int id, IntPtr pFacility, int amount) {
        var simu = GetSimulator(id);
        var facility = Marshal.PtrToStringUTF8(pFacility) ?? string.Empty;
        simu.UseDrones(facility, amount);
    }

    [UnmanagedCallersOnly(EntryPoint = "Sanity")]
    public static void Sanity(int id, int amount) {
        var simu = GetSimulator(id);
        simu.AddDrones(amount);
    }

    [UnmanagedCallersOnly(EntryPoint = "GetDataForMower")]
    public static IntPtr GetDataForMower(int id) {
        var simu = GetSimulator(id);
        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, true);
        writer.Flush();
        ms.Position = 0;
        var node = JsonNode.Parse(ms);
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


        ms.Position = 0;
        using var writer2 = new Utf8JsonWriter(ms);
        node.WriteTo(writer2);
        ms.WriteByte(0);

        var ptr = Marshal.AllocHGlobal((int)ms.Length);
        Marshal.Copy(ms.GetBuffer(), 0, ptr, (int)ms.Length);
        return ptr;
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
