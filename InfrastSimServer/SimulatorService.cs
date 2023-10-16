using InfrastSim.TimeDriven;
using System.Collections.Concurrent;
using System.Text.Json;

namespace InfrastSimServer; 
public class SimulatorService {
    //public static readonly SimulatorService Instance = new(); 

    private int _simuId = 0;
    private ConcurrentDictionary<int, TimeDrivenSimulator> _simus = new();

    public int Create(HttpContext httpContext) {
        var id = Interlocked.Increment(ref _simuId);
        _simus[_simuId] = new TimeDrivenSimulator();
        return id;
    }

    public int CreateWithData(HttpContext httpContext) {
        var doc = JsonDocument.Parse(httpContext.Request.Body);
        var id = Interlocked.Increment(ref _simuId);
        _simus[_simuId] = new TimeDrivenSimulator(doc.RootElement);
        return id;
    }
}
