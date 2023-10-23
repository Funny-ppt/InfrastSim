using InfrastSim.TimeDriven;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InfrastSim.CDLL;
public static class SimulatorService {
    private static int SimuId = 0;
    private static ConcurrentDictionary<int, Simulator> Simus = new();

    static Simulator GetSimulator(int id) {
        if (!Simus.TryGetValue(id, out var simulator)) {
            throw new KeyNotFoundException();
        }
        return simulator;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateSimulator")]
    public static int Create() {
        var id = Interlocked.Increment(ref SimuId);
        Simus[id] = new Simulator();
        return id;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateSimulatorWithData")]
    public static int CreateWithData(IntPtr pJson) {
        var json = Marshal.PtrToStringUTF8(pJson) ?? string.Empty;
        var doc = JsonDocument.Parse(json);
        var id = Interlocked.Increment(ref SimuId);
        Simus[id] = new Simulator(doc.RootElement);
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
        var simu = GetSimulator(id);

        simu.Resolve();
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(simu, detailed);
        writer.Flush();
        ms.WriteByte(0);

        var ptr = Marshal.AllocHGlobal((int)ms.Length);
        Marshal.Copy(ms.GetBuffer(), 0, ptr, (int)ms.Length);
        return ptr;
    }


    [UnmanagedCallersOnly(EntryPoint = "Simulate")]
    public static void Simulate(int id, int seconds) {
        if (!Simus.TryGetValue(id, out var simu)) {
            return;
        }

        simu.SimulateUntil(simu.Now + TimeSpan.FromSeconds(seconds));
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
    public static int UseDrones(int id, IntPtr pFacility, int amount) {
        var simu = GetSimulator(id);
        var facility = Marshal.PtrToStringUTF8(pFacility) ?? string.Empty;
        return simu.UseDrones(facility, amount);
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
        MowerHelper.RewriteSimulator(simu, node);

        ms.Position = 0;
        using var writer2 = new Utf8JsonWriter(ms);
        node.WriteTo(writer2);
        writer.Flush();
        ms.WriteByte(0);

        var ptr = Marshal.AllocHGlobal((int)ms.Length);
        Marshal.Copy(ms.GetBuffer(), 0, ptr, (int)ms.Length);
        return ptr;
    }
}
