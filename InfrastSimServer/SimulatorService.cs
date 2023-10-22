using InfrastSim;
using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InfrastSimServer; 
public class SimulatorService : IDisposable {
    //public static readonly SimulatorService Instance = new(); 

    private Timer? _timer;
    private int _simuId = 0;
    private ConcurrentDictionary<int, Simulator> _simus = new();
    private ConcurrentDictionary<int, DateTime> _lastAccess = new();

    public SimulatorService(bool cleanup = false) {
        if (cleanup) _timer = new Timer(Cleanup, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }

    Simulator GetSimulator(int id) {
        if (!_simus.TryGetValue(id, out var simulator)) {
            throw new NotFoundException();
        }
        _lastAccess[id] = DateTime.Now;
        return simulator;
    }

    public void Create(HttpContext httpContext) {
        var id = Interlocked.Increment(ref _simuId);
        var simu = _simus[id] = new Simulator();
        _lastAccess[id] = DateTime.Now;

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
        var simu = _simus[id] = new Simulator(doc.RootElement);
        _lastAccess[id] = DateTime.Now;

        httpContext.Response.ContentType = "application/json";
        using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        writer.WriteStartObject();
        writer.WriteNumber("id", id);
        writer.WriteItem("data", simu);
        writer.WriteEndObject();
        writer.Flush();
    }

    public void Destory(HttpContext httpContext, int id) {
        if (!_simus.ContainsKey(id)) {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        _simus.TryRemove(id, out _);
        _lastAccess.TryRemove(id, out _);
    }
    public void GetData(HttpContext httpContext, int id, bool detailed = true) {
        var simu = GetSimulator(id);

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
    }

    public void SimulateP(HttpContext httpContext, int id, SimulateData data) {
        var simu = GetSimulator(id);

        var time = new TimeSpan(data.Hours, data.Minutes, data.Seconds);
        var span = TimeSpan.FromSeconds(data.TimeSpan);
        simu.SimulateUntil(simu.Now + time, span);
    }

    public async Task SetFacilityState(HttpContext httpContext, int id, string facility) {
        var simu = GetSimulator(id);
        var doc = await JsonDocument.ParseAsync(httpContext.Request.Body);
        simu.SetFacilityState(facility, doc.RootElement);
    }

    public void GetOperators(HttpContext httpContext, int id) {
        var simu = GetSimulator(id);
        httpContext.Response.ContentType = "application/json";
        using var writer = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        writer.WriteOperators(simu);
        writer.Flush();
    }

    public void SetUpgraded(HttpContext httpContext, int id, Dictionary<string, int> ops) {
        var simu = GetSimulator(id);
        foreach (var kvp in ops) {
            simu.SetUpgraded(kvp.Key, kvp.Value);
        }
    }

    public void SelectOperators(HttpContext httpContext, int id, SelectOperatorsData data) {
        var simu = GetSimulator(id);
        simu.SelectOperators(data.Facility, data.Operators);
    }

    public void RemoveOperator(HttpContext httpContext, int id, string facility, int idx) {
        var simu = GetSimulator(id);
        simu.RemoveOperator(facility, idx);
    }

    public void RemoveOperators(HttpContext httpContext, int id, string facility) {
        var simu = GetSimulator(id);
        simu.RemoveOperators(facility);
    }

    public void CollectAll(HttpContext httpContext, int id) {
        var simu = GetSimulator(id);
        simu.CollectAll();
    }

    public void Collect(HttpContext httpContext, int id, string facility, int idx = 0) {
        var simu = GetSimulator(id);
        simu.Collect(facility, idx);
    }

    public void UseDrones(HttpContext httpContext, int id, string facility, int amount) {
        var simu = GetSimulator(id);
        simu.UseDrones(facility, amount);
    }

    public void Sanity(HttpContext httpContext, int id, int amount) {
        var simu = GetSimulator(id);
        simu.AddDrones(amount);
    }

    public void SimulateUntil(HttpContext httpContext, int id, DateTime until) {
        var simu = GetSimulator(id);
        simu.SimulateUntil(until, TimeSpan.FromMinutes(1));
        httpContext.Response.ContentType = "application/json";
    }

    public async Task GetDataForMower(HttpContext httpContext, int id) {
        var simu = GetSimulator(id);
        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, true);
        writer.Flush();
        ms.Position = 0;
        var node = await JsonNode.ParseAsync(ms);
        node = MowerHelper.RewriteSimulator(simu, node);


        httpContext.Response.ContentType = "application/json";
        using var respWriter = new Utf8JsonWriter(httpContext.Response.BodyWriter.AsStream());
        node.WriteTo(respWriter);
        writer.Flush();
    }

    void Cleanup(object? state) {
        var simToCleanup = _lastAccess
            .Where(kvp => DateTime.Now - kvp.Value > TimeSpan.FromDays(1))
            .Select(kvp => kvp.Key);
        foreach (var id in simToCleanup) {
            _simus.TryRemove(id, out _);
            _lastAccess.TryRemove(id, out _);
        }
    }

    public void Dispose() {
        _timer?.Dispose();
    }

    //public void SaveTo(string path) {

    //}
}
