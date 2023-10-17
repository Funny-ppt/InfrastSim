using System.Runtime.InteropServices;

namespace InfrastSim;
public static class Exports {
    static int _cnt = 0;
    static readonly Dictionary<int, ISimulator> _map = new();

    [UnmanagedCallersOnly(EntryPoint = "CreateTimeDrivenSimulator")]
    public static int CreateTimeDrivenSimulator() {
        var id = Interlocked.Increment(ref _cnt);
        _map[id] = new TimeDriven.Simulator();
        return id;
    }

    [UnmanagedCallersOnly(EntryPoint = "CreateTimeDrivenSimulator")]
    public static bool DestroySimulator(int id) {
        if (_map.ContainsKey(id)) {
            _map.Remove(id);
        }
        return false;
    }
}
