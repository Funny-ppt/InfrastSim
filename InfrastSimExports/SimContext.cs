using InfrastSim.TimeDriven;

namespace InfrastSim.CDLL;
internal struct SimContext {
    public int Id;
    public MemoryStream OutputStream;
    public Simulator Simulator;
    public SimContext(int id, Simulator simulator) {
        Id = id;
        OutputStream = new();
        Simulator = simulator;
    }
}
