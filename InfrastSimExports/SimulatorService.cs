using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InfrastSim.CDLL;
public static class SimulatorService {
    private static int SimuId = 0;
    private static ConcurrentDictionary<int, SimContext> Simus = new();

    static Simulator GetSimulator(int id) {
        if (!Simus.TryGetValue(id, out var context)) {
            throw new KeyNotFoundException();
        }
        return context.Simulator;
    }
    static SimContext GetContext(int id) {
        if (!Simus.TryGetValue(id, out var context)) {
            throw new KeyNotFoundException();
        }
        return context;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateSimulator")]
    public static int Create() {
        var id = Interlocked.Increment(ref SimuId);
        Simus[id] = new(id, new Simulator());
        return id;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateSimulatorWithData")]
    public static int CreateWithData(IntPtr pJson, bool newRandom) {
        // 长JSON输入还是没什么好办法，该传指针接着传

        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);
        var id = Interlocked.Increment(ref SimuId);
        var simu = new Simulator(doc.RootElement);
        Simus[id] = new(id, simu);
        if (newRandom) simu.Random = new();
        return id;
    }

    [UnmanagedCallersOnly(EntryPoint = "DestroySimulator")]
    public static bool Destory(int id) {
        if (!Simus.ContainsKey(id)) {
            return false;
        }

        return Simus.TryRemove(id, out _);
    }

    [UnmanagedCallersOnly(EntryPoint = "GetData")]
    public static IntPtr GetData(int id, bool detailed = true) {
        var context = GetContext(id);
        var simu = context.Simulator;
        simu.Resolve();

        var ms = context.OutputStream;
        ms.Position = 0;
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, detailed);
        writer.Flush();
        ms.WriteByte(0);

        return context.GetBuffer();
    }


    [UnmanagedCallersOnly(EntryPoint = "Simulate")]
    public static void Simulate(int id, int seconds) {
        var simu = GetSimulator(id);

        simu.SimulateUntil(simu.Now + TimeSpan.FromSeconds(seconds));
    }


    [UnmanagedCallersOnly(EntryPoint = "SetFacilityState")]
    public static void SetFacilityState(int id, Facility facility, IntPtr pJson) {
        var simu = GetSimulator(id);

        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);
        simu.SetFacilityState(facility.ToString(), doc.RootElement);
    }

    [UnmanagedCallersOnly(EntryPoint = "SetFacilitiesState")]
    public static void SetFacilitesState(int id, IntPtr pJson) {
        var simu = GetSimulator(id);

        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject()) {
            simu.SetFacilityState(prop.Name, prop.Value);
        }
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

    [UnmanagedCallersOnly(EntryPoint = "SetLevel")]
    public static void SetLevel(int id, Facility facility, int level) {
        var simu = GetSimulator(id);
        var fac = simu.GetFacilityByIndex((int)facility)
            ?? throw new ArgumentException($"{facility} 对应的设施未建造");
        fac.SetLevel(level);
    }

    [UnmanagedCallersOnly(EntryPoint = "SetStrategy")]
    public static void SetStrategy(int id, Facility facility, TradingStation.OrderStrategy strategy) {
        var simu = GetSimulator(id);
        var fac = simu.GetFacilityByIndex((int)facility);
        if (fac is TradingStation trading) {
            trading.Strategy = strategy;
        } else {
            throw new ArgumentException($"{facility} 对应的设施不是贸易站或未建造");
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "SetProduct")]
    public static void SetProduct(int id, Facility facility, Product product) {
        var simu = GetSimulator(id);
        var fac = simu.GetFacilityByIndex((int)facility);
        if (fac is ManufacturingStation manufacturing) {
            manufacturing.ChangeProduct(InfrastSim.Product.AllProducts[(int)product]);
        } else {
            throw new ArgumentException($"{facility} 对应的设施不是制造站或未建造");
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "RemoveOperator")]
    public static void RemoveOperator(int id, Facility facility, int idx) {
        var simu = GetSimulator(id);
        simu.RemoveOperator(facility.ToString(), idx);
    }

    [UnmanagedCallersOnly(EntryPoint = "RemoveOperators")]
    public static void RemoveOperators(int id, Facility facility) {
        var simu = GetSimulator(id);
        simu.RemoveOperators(facility.ToString());
    }

    [UnmanagedCallersOnly(EntryPoint = "CollectAll")]
    public static void CollectAll(int id) {
        var simu = GetSimulator(id);
        simu.CollectAll();
    }

    [UnmanagedCallersOnly(EntryPoint = "Collect")]
    public static void Collect(int id, Facility facility, int idx = 0) {
        var simu = GetSimulator(id);
        simu.Collect(facility.ToString(), idx);
    }

    [UnmanagedCallersOnly(EntryPoint = "UseDrones")]
    public static int UseDrones(int id, Facility facility, int amount) {
        var simu = GetSimulator(id);
        return simu.UseDrones(facility.ToString(), amount);
    }

    [UnmanagedCallersOnly(EntryPoint = "Sanity")]
    public static void Sanity(int id, int amount) {
        var simu = GetSimulator(id);
        simu.AddDrones(amount);
    }

    [UnmanagedCallersOnly(EntryPoint = "GetDataForMower")]
    public static IntPtr GetDataForMower(int id) {
        var context = GetContext(id);
        var simu = context.Simulator;
        simu.Resolve();

        var ms = context.OutputStream;
        ms.Position = 0;
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
        ms.WriteByte(0);

        return context.GetBuffer();
    }

    static MemoryStream EnumerateSharedMS = new();
    static GCHandle EnumerateSharedGCHandle;
    [UnmanagedCallersOnly(EntryPoint = "EnumerateGroup")]
    public static IntPtr EnumerateGroup(IntPtr data) {
        EnumerateSharedMS.Position = 0;
        using var writer = new Utf8JsonWriter(EnumerateSharedMS);
        var json = Marshal.PtrToStringUTF8(data);
        var doc = JsonDocument.Parse(json);

        EnumerateHelper.Enumerate(doc, writer);
        EnumerateSharedMS.WriteByte(0);

        if (EnumerateSharedGCHandle.IsAllocated) {
            EnumerateSharedGCHandle.Free();
        }

        var buffer = EnumerateSharedMS.GetBuffer();
        EnumerateSharedGCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        return EnumerateSharedGCHandle.AddrOfPinnedObject();
    }
}
