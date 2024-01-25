using InfrastSim.Localization;
using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace InfrastSim.Wasm;

[SupportedOSPlatform("browser")]
public static unsafe partial class SimulatorService {
    private static int SimuId = 0;
    private static ConcurrentDictionary<int, Simulator> Simus = new();

    public static void Main() {
        // no init required
    }

    static Simulator GetSimulator(int id) {
        if (!Simus.TryGetValue(id, out var simulator)) {
            throw new KeyNotFoundException();
        }
        return simulator;
    }

    [JSExport]
    public static int Create() {
        var id = Interlocked.Increment(ref SimuId);
        Simus[id] = new Simulator();
        return id;
    }

    [JSExport]
    public static int CreateWithData(string json, bool newRandom = false) {
        var doc = JsonDocument.Parse(json);
        var id = Interlocked.Increment(ref SimuId);
        var simu = Simus[id] = new Simulator(doc.RootElement);
        if (newRandom) simu.Random = new RandomEx.XoshiroRandom();
        return id;
    }

    [JSExport]
    public static bool Destory(int id) {
        if (!Simus.ContainsKey(id)) {
            return false;
        }

        return Simus.TryRemove(id, out _);
    }

    [JSExport]
    public static string GetData(int id, bool detailed = true) {
        var simu = GetSimulator(id);

        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }


    [JSExport]
    public static void Simulate(int id, int seconds = 0) {
        if (!Simus.TryGetValue(id, out var simu)) {
            return;
        }
        if (seconds == 0) {
            seconds = 60;
        }

        simu.SimulateUntil(simu.Now + TimeSpan.FromSeconds(seconds));
    }


    [JSExport]
    public static void SetFacilityState(int id, string facility, string json) {
        var simu = GetSimulator(id);
        var doc = JsonDocument.Parse(json);
        simu.SetFacilityState(facility, doc.RootElement);
    }

    [JSExport]
    public static void SetFacilitiesState(int id, string json) {
        var simu = GetSimulator(id);
        var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject()) {
            simu.SetFacilityState(prop.Name, prop.Value);
        }
    }

    [JSExport]
    public static string GetOperators(int id) {
        var simu = GetSimulator(id);
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteOperators(simu);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }


    [JSExport]
    public static void SetUpgraded(int id, string json) {
        var simu = GetSimulator(id);
        var doc = JsonDocument.Parse(json);

        foreach (var prop in doc.RootElement.EnumerateObject()) {
            simu.SetUpgraded(prop.Name, prop.Value.GetInt32());
        }
    }


    [JSExport]
    public static void RemoveOperator(int id, string facility, int idx) {
        var simu = GetSimulator(id);
        simu.RemoveOperator(facility, idx);
    }

    [JSExport]
    public static void RemoveOperators(int id, string facility) {
        var simu = GetSimulator(id);
        simu.RemoveOperators(facility);
    }

    [JSExport]
    public static void CollectAll(int id) {
        var simu = GetSimulator(id);
        simu.CollectAll();
    }

    [JSExport]
    public static void Collect(int id, string facility, int idx = 0) {
        var simu = GetSimulator(id);
        simu.Collect(facility, idx);
    }


    [JSExport]
    public static int UseDrones(int id, string facility, int amount) {
        var simu = GetSimulator(id);
        return simu.UseDrones(facility, amount);
    }

    [JSExport]
    public static void Sanity(int id, int amount) {
        var simu = GetSimulator(id);
        simu.AddDrones(amount);
    }

    [JSExport]
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


    [JSExport]
    public static string EnumerateGroup(string json) {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        var doc = JsonDocument.Parse(json);

        EnumerateHelper.Enumerate(doc, writer);
        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }

    [JSExport]
    public static void ExecuteScript(int id, string script) {
        var simu = GetSimulator(id);
        Helper.ExecuteScript(simu, script);
    }

    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(string))]
    public partial class SerializeContext : JsonSerializerContext { }

    //[JSExport]
    //public static string GetScriptInfo() {
    //    var commands = Helper.GetCommands();
    //    var languages = Helper.GetLanguages();
    //    var mappings = new Dictionary<string, Dictionary<string, string>>();
    //    foreach (var language in languages) {
    //        mappings[language] = Helper.GetCommandMappings(Enum.Parse<Language>(language));
    //    }
    //}
}
