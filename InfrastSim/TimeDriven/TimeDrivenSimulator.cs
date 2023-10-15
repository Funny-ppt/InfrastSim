using System.Diagnostics;

namespace InfrastSim.TimeDriven;
internal class TimeDrivenSimulator : ISimulator {
    public DateTime Now { get; private set; }
    public void Simulate(TimeSpan span) {
        foreach (var value in _globalValues.Values) {
            value.Clear();
        }
        foreach (var facility in AllFacilities) {
            facility.Reset();
        }
        foreach (var facility in AllFacilities) {
            facility.Resolve(this);
        }
        _delayActions?.Invoke(this);
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
    public IEnumerable<PowerStation> PowerStations
        => ModifiableFacilities.Select(fac => fac as PowerStation).Where(fac => fac != null);
    public IEnumerable<TradingStation> TradingStations
        => ModifiableFacilities.Select(fac => fac as TradingStation).Where(fac => fac != null);
    public IEnumerable<ManufacturingStation> ManufacturingStation
        => ModifiableFacilities.Select(fac => fac as ManufacturingStation).Where(fac => fac != null);
    public FacilityBase[] AllFacilities { get; } = new FacilityBase[18];
    public IEnumerable<OperatorBase> Operators => AllFacilities.SelectMany(fac => fac.Operators);

    double _drones;
    Dictionary<string, int> _materials = new();
    Dictionary<string, AggregateValue> _globalValues = new();
    Action<TimeDrivenSimulator>? _delayActions = null;

    public void DelayAction(Action<TimeDrivenSimulator> action) {
        _delayActions += action;
    }
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
        if (!_globalValues.ContainsKey(name)) {
            _globalValues.Add(name, new(min: 0.0));
        }
        return _globalValues[name];
    }

    public AggregateValue SilverVine => GetGlobalValue(nameof(SilverVine));
    public AggregateValue Renjianyanhuo => GetGlobalValue(nameof(Renjianyanhuo));
    public AggregateValue Ganzhixinxi => GetGlobalValue(nameof(Ganzhixinxi));
    public AggregateValue ExtraGoldProductionLine => GetGlobalValue(nameof(ExtraGoldProductionLine));
    public AggregateValue ExtraPowerStation => GetGlobalValue(nameof(ExtraPowerStation));
    public AggregateValue GlobalManufacturingEffiency => GetGlobalValue(nameof(GlobalManufacturingEffiency));
    public AggregateValue GlobalTradingEffiency => GetGlobalValue(nameof(GlobalTradingEffiency));
}
