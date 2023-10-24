using RandomEx;
using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public class Simulator : ISimulator, IJsonSerializable {
    public Simulator() {
        Now = DateTime.UtcNow;
        Random = new XoshiroRandom();
        AllFacilities[0] = ControlCenter = new();
        AllFacilities[1] = Office = new();
        AllFacilities[2] = Reception = new();
        AllFacilities[3] = Training = new();
        AllFacilities[4] = Crafting = new();
        for (int i = 5; i < 9; i++) {
            AllFacilities[i] = new Dormitory();
        }

        Operators = OperatorInstances.Operators.ToDictionary(kvp =>  kvp.Key, kvp => kvp.Value.Clone());
    }

    public Simulator(JsonElement elem) {
        Now = elem.GetProperty("time").GetDateTime();
        Random = new XoshiroRandom(elem.GetProperty("random"));
        _drones = elem.GetProperty("drones").GetDouble();

        foreach (var prop in elem.GetProperty("materials").EnumerateObject()) {
            _materials.Add(prop.Name, prop.Value.GetInt32());
        }

        Operators = new();
        foreach (var op_elem in elem.GetProperty("operators").EnumerateArray()) {
            var name = op_elem.GetProperty("name").GetString();
            Operators[name] = OperatorBase.FromJson(op_elem);
        }
        var newops = OperatorInstances.Operators
            .ExceptBy(Operators.Keys, kvp => kvp.Key);
        foreach (var kvp in newops) {
            Operators[kvp.Key] = kvp.Value.Clone();
        }

        AllFacilities[0] = ControlCenter = FacilityBase.FromJson(elem.GetProperty("control-center"), this) as ControlCenter;
        AllFacilities[1] = Office = FacilityBase.FromJson(elem.GetProperty("office"), this) as Office;
        AllFacilities[2] = Reception = FacilityBase.FromJson(elem.GetProperty("reception"), this) as Reception;
        AllFacilities[3] = Training = FacilityBase.FromJson(elem.GetProperty("training"), this) as Training;
        AllFacilities[4] = Crafting = FacilityBase.FromJson(elem.GetProperty("crafting"), this) as Crafting;
        int i = 5;
        foreach (var dormElem in elem.GetProperty("dormitories").EnumerateArray()) {
            var dorm = FacilityBase.FromJson(dormElem, this);
            AllFacilities[i++] = dorm;
        }
        foreach (var facElem in elem.GetProperty("modifiable-facilities").EnumerateArray()) {
            var fac = FacilityBase.FromJson(facElem, this);
            AllFacilities[i++] = fac;
        }
    }

    public DateTime Now { get; private set; }
    public XoshiroRandom Random { get; set; }
    ITimeDrivenObject? _interestSource;
    TimeSpan _nextInterest;
    TimeSpan _minSpan = TimeSpan.FromSeconds(10);
    internal void SetInterest(ITimeDrivenObject o, TimeSpan span) {
        if (span < _nextInterest) {
            _interestSource = o;
            _nextInterest = span;
        }
    }
    public void Resolve() {
        foreach (var value in _globalValues.Values) {
            value.Clear();
        }
        foreach (var facility in AllFacilities) {
            facility?.Reset();
        }
        foreach (var facility in AllFacilities) {
            facility?.Resolve(this);
        }
        foreach (var actions in _delayActions.Values) {
            actions(this);
        }
        _delayActions.Clear();
    }
    void QueryInterest() {
        foreach (var facility in AllFacilities) {
            facility?.QueryInterest(this);
        }
    }
    void SimulateImpl(TimeSpan span) {
        var info = new TimeElapsedInfo(Now, Now + span, span);
        foreach (var facility in AllFacilities) {
            facility?.Update(this, info);
        }
        AddDrones((1 + GlobalDronesEffiency) * (info.TimeElapsed / TimeSpan.FromMinutes(6)));
        Now += span;
    }
    public void SimulateUntil(DateTime dateTime) {
        while (Now < dateTime) {
            Resolve();
            QueryInterest();
            var span = dateTime - Now;

            if (span < _minSpan) {
                SimulateImpl(span);
                return;
            }

            if (_nextInterest < span) span = _nextInterest;
            if (_minSpan > span) span = _minSpan;
            SimulateImpl(span);
        }
    }

    internal Dictionary<string, OperatorBase> Operators;
    internal OperatorBase GetOperator(string name) {
        return Operators.GetValueOrDefault(name) ?? throw new KeyNotFoundException($"未知的干员名称 {name}");
    }
    internal OperatorBase? GetOperatorNoThrow(string name) {
        return Operators.GetValueOrDefault(name);
    }

    public ControlCenter ControlCenter {
        get => (ControlCenter)AllFacilities[0]!;
        private set => AllFacilities[0] = value;
    }
    public Office Office {
        get => (Office)AllFacilities[1]!;
        private set => AllFacilities[1] = value;
    }
    public Reception Reception {
        get => (Reception)AllFacilities[2]!;
        private set => AllFacilities[3] = value;
    }
    public Training Training {
        get => (Training)AllFacilities[3]!;
        private set => AllFacilities[3] = value;
    }
    public Crafting Crafting {
        get => (Crafting)AllFacilities[4]!;
        private set => AllFacilities[4] = value;
    }
    public ArraySegment<FacilityBase?> Dormitories {
        get => new(AllFacilities, 5, 4);
    }
    public ArraySegment<FacilityBase?> ModifiableFacilities {
        get => new(AllFacilities, 9, 9);
    }
    internal FacilityBase?[] AllFacilities { get; } = new FacilityBase?[18];

    public IEnumerable<PowerStation> PowerStations
        => ModifiableFacilities.Select(fac => fac as PowerStation).Where(fac => fac != null);
    public IEnumerable<TradingStation> TradingStations
        => ModifiableFacilities.Select(fac => fac as TradingStation).Where(fac => fac != null);
    public IEnumerable<ManufacturingStation> ManufacturingStations
        => ModifiableFacilities.Select(fac => fac as ManufacturingStation).Where(fac => fac != null);
    public IEnumerable<OperatorBase> OperatorsInFacility
        => AllFacilities.SelectMany(fac => fac?.Operators ?? Enumerable.Empty<OperatorBase>());

    public int TotalPowerConsume =>
        AllFacilities
        .Where(fac => fac != null && fac is not PowerStation)
        .Sum(fac => fac!.PowerConsumes);

    public int TotalPowerProduce =>
        ModifiableFacilities
        .Where(fac => fac is PowerStation)
        .Sum(fac => -fac!.PowerConsumes);
    public double NextDroneTimeInSeconds =>
        (Math.Ceiling(_drones) - _drones) * 360 / (1 + GlobalDronesEffiency);

    double _drones;
    Dictionary<string, int> _materials = new();
    Dictionary<string, AggregateValue> _globalValues = new();
    SortedDictionary<int, Action<Simulator>> _delayActions = new();

    public void Delay(Action<Simulator> action, int priority = 100) {
        if (_delayActions.ContainsKey(priority)) {
            _delayActions[priority] += action;
        } else {
            _delayActions[priority] = action;
        }
    }
    public int Drones => (int)Math.Floor(_drones);
    public void AddDrones(double amount) => _drones = Math.Min(200, _drones + amount);

    internal void RemoveDrones(int amount) => _drones -= Math.Max(Drones, amount);
    internal void RemoveMaterial(Material mat) {
        _materials[mat.Name] = _materials.GetValueOrDefault(mat.Name) - mat.Count;
    }
    internal void RemoveMaterial(Material mat, int multiply) {
        _materials[mat.Name] = _materials.GetValueOrDefault(mat.Name) - mat.Count * multiply;
    }
    internal void AddMaterial(Material mat) {
        _materials[mat.Name] = _materials.GetValueOrDefault(mat.Name) + mat.Count;
    }
    internal void AddMaterial(string name, int amount) {
        _materials[name] = _materials.GetValueOrDefault(name) + amount;
    }

    public AggregateValue GetGlobalValue(string name) {
        if (!_globalValues.ContainsKey(name)) {
            _globalValues.Add(name, new(min: 0.0));
        }
        return _globalValues[name];
    }

    public IEnumerable<(string, double)> GlobalValues
        => _globalValues.Select(kvp => (kvp.Key, (double)kvp.Value));
    public AggregateValue SilverVine => GetGlobalValue("木天蓼");
    public AggregateValue Renjianyanhuo => GetGlobalValue("人间烟火");
    public AggregateValue Ganzhixinxi => GetGlobalValue("感知信息");
    public AggregateValue Wushenggongming => GetGlobalValue("无声共鸣");
    public AggregateValue Siweilianhuan => GetGlobalValue("思维链环");
    public AggregateValue Gongchengjiqiren => GetGlobalValue("工程机器人");
    public AggregateValue ExtraGoldProductionLine => GetGlobalValue("虚拟赤金线");
    public AggregateValue ExtraPowerStation => GetGlobalValue("虚拟发电站");
    public AggregateValue GlobalManufacturingEffiency => GetGlobalValue(nameof(GlobalManufacturingEffiency));
    public AggregateValue GlobalTradingEffiency => GetGlobalValue(nameof(GlobalTradingEffiency));
    public AggregateValue GlobalDronesEffiency => GetGlobalValue(nameof(GlobalDronesEffiency));

    public string ToJson(bool detailed = false) {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        ToJson(writer, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray()) ?? string.Empty;
    }
    public void ToJson(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteStartObject();

        writer.WriteString("time", Now);
        writer.WritePropertyName("random");
        Random.ToJson(writer);
        writer.WriteNumber("drones", _drones);

        writer.WritePropertyName("operators");
        writer.WriteStartArray();
        foreach (var op in Operators.Values) {
            writer.WriteItemValue(op, detailed);
        }
        writer.WriteEndArray();

        writer.WritePropertyName("materials");
        writer.WriteStartObject();
        foreach (var kvp in _materials) {
            writer.WriteNumber(kvp.Key, kvp.Value);
        }
        writer.WriteEndObject();

        writer.WriteItem("control-center", ControlCenter, detailed);
        writer.WriteItem("office", Office, detailed);
        writer.WriteItem("reception", Reception, detailed);
        writer.WriteItem("training", Training, detailed);
        writer.WriteItem("crafting", Crafting, detailed);

        writer.WritePropertyName("dormitories");
        writer.WriteStartArray();
        foreach (var fac in Dormitories) {
            if (fac == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteItemValue(fac, detailed);
            }
        }
        writer.WriteEndArray();

        writer.WritePropertyName("modifiable-facilities");
        writer.WriteStartArray();
        foreach (var fac in ModifiableFacilities) {
            if (fac == null) {
                writer.WriteNullValue();
            } else {
                writer.WriteItemValue(fac, detailed);
            }
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
