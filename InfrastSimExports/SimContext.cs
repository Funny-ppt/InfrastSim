using InfrastSim.TimeDriven;
using System.Runtime.InteropServices;

namespace InfrastSim.CDLL;
internal class SimContext {
    public int Id;
    public MemoryStream OutputStream;
    public GCHandle GCHandle;
    public Simulator Simulator;
    public SimContext(int id, Simulator simulator) {
        Id = id;
        OutputStream = new();
        Simulator = simulator;
    }

    public IntPtr GetBuffer() {
        if (GCHandle.IsAllocated) {
            GCHandle.Free();
        }

        var buffer = OutputStream.GetBuffer();
        GCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        return GCHandle.AddrOfPinnedObject();
    }
}
