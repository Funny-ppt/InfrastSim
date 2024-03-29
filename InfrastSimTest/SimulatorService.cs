using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InfrastSim.Wasm;

public static class SimulatorService {
    private static int SimuId = 0;
    private static ConcurrentDictionary<int, Simulator> Simus = new();

    static void EnsurePropExists(Simulator simu) {
        /// 如果没有干员访问这两个属性，在Resolve中就不会产生，但Update中制造站和贸易站始终会访问这两个值，
        /// 进而导致出现默认的值，使得序列化、反序列化结果不一致（尽管没有任何影响）
        var p1 = simu.GlobalManufacturingEfficiency;
        var p2 = simu.GlobalTradingEfficiency;
    }

    static Simulator GetSimulator(int id) {
        if (!Simus.TryGetValue(id, out var simulator)) {
            throw new KeyNotFoundException();
        }
        return simulator;
    }

    public static int Create() {
        var id = Interlocked.Increment(ref SimuId);
        Simus[id] = new Simulator();
        return id;
    }

    public static int CreateWithData(string json, bool newRandom = false) {
        var doc = JsonDocument.Parse(json);
        var id = Interlocked.Increment(ref SimuId);
        var simu = Simus[id] = new Simulator(doc.RootElement);
        if (newRandom) simu.Random = new();
        return id;
    }


    public static bool Destory(int id) {
        if (!Simus.ContainsKey(id)) {
            return false;
        }

        return Simus.TryRemove(id, out _);
    }


    public static string GetData(int id, bool detailed = true) {
        var simu = GetSimulator(id);

        EnsurePropExists(simu);
        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }



    public static void Simulate(int id, int seconds = 0) {
        if (!Simus.TryGetValue(id, out var simu)) {
            return;
        }
        if (seconds == 0) {
            seconds = 60;
        }

        simu.SimulateUntil(simu.Now + TimeSpan.FromSeconds(seconds));
    }



    public static void SetFacilityState(int id, string facility, string json) {
        var simu = GetSimulator(id);
        var doc = JsonDocument.Parse(json);
        simu.SetFacilityState(facility, doc.RootElement);
    }



    public static string GetOperators(int id) {
        var simu = GetSimulator(id);
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteOperators(simu);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }



    public static void SetUpgraded(int id, string json) {
        var simu = GetSimulator(id);
        var doc = JsonDocument.Parse(json);

        foreach (var prop in doc.RootElement.EnumerateObject()) {
            simu.SetUpgraded(prop.Name, prop.Value.GetInt32());
        }
    }



    public static void RemoveOperator(int id, string facility, int idx) {
        var simu = GetSimulator(id);
        simu.RemoveOperator(facility, idx);
    }


    public static void RemoveOperators(int id, string facility) {
        var simu = GetSimulator(id);
        simu.RemoveOperators(facility);
    }


    public static void CollectAll(int id) {
        var simu = GetSimulator(id);
        simu.CollectAll();
    }


    public static void Collect(int id, string facility, int idx = 0) {
        var simu = GetSimulator(id);
        simu.Collect(facility, idx);
    }



    public static int UseDrones(int id, string facility, int amount) {
        var simu = GetSimulator(id);
        return simu.UseDrones(facility, amount);
    }


    public static void Sanity(int id, int amount) {
        var simu = GetSimulator(id);
        simu.AddDrones(amount);
    }


    public static string GetDataForMower(int id) {
        var simu = GetSimulator(id);
        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, true);
        writer.Flush();
        ms.Position = 0;
        var node = JsonNode.Parse(ms);
        MowerHelper.RewriteSimulator(simu, node);

        ms.Position = 0;
        using var writer2 = new Utf8JsonWriter(ms);
        node.WriteTo(writer2);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }
}