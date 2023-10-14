using System;
using System.Diagnostics;

namespace InfrastSim.TimeDriven;
internal class TimeDrivenSimulator : ISimulator {
    public DateTime Now { get; private set; }
    public void Simulate(TimeSpan span) {
        foreach (var facility in AllFacilities) {
            facility.Resolve(this);
        }
        foreach (var facility in AllFacilities) {
            facility.Update(this, new TimeElapsedInfo(Now, Now + span, span));
        }
        Now += span;
    }
    public void SimulateUntil(DateTime dateTime, TimeSpan interval) {
        while (Now < dateTime) {
            Simulate(interval);
        }
    }


    public ControlCenter ControlCenter = new ();
    public Dormitory[] Dormitories = new Dormitory[4];
    public Office Office = new();
    public Reception Reception = new();
    public Training Training = new();
    public Crafting Crafting = new();
    public FacilityBase[] ModifiableFacilities { get; } = new FacilityBase[9];
    public FacilityBase[] AllFacilities { get; } = new FacilityBase[18];

    double _drones;
    Dictionary<string, int> _materials = new();
    Dictionary<string, AggregateValue> _globalStates = new();

    public int Drones {
        get => (int)Math.Floor(_drones);
        set {
            Debug.Assert(value >= 0 && value <= Drones);
            _drones -= Drones - value;
        }
    }
    public void AddDrones(double amount) => _drones = Math.Min(200, _drones + amount);
    public void ConsumeMaterial(Material mat) {
        _materials[mat.Name] = _materials.GetValueOrDefault(mat.Name) - mat.Count;
    }
    public AggregateValue GetGlobalValue(string name) {
        if (!_globalStates.ContainsKey(name)) {
            _globalStates.Add(name, new(min: 0.0));
        }
        return _globalStates[name];
    }

    public AggregateValue SilverVine { get; } = new AggregateValue();
    public AggregateValue GlobalManufacturingEffiency { get; } = new AggregateValue();
    public AggregateValue GlobalTradingEffiency { get; } = new AggregateValue();
}
