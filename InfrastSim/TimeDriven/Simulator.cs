using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public class Simulator : ISimulator, IJsonSerializable {
    public Simulator() {
        Now = DateTime.UtcNow;
        AllFacilities[0] = ControlCenter = new();
        AllFacilities[1] = Office = new();
        AllFacilities[2] = Reception = new();
        AllFacilities[3] = Training = new();
        AllFacilities[4] = Crafting = new();

        Operators = OperatorInstances.Operators.ToDictionary(kvp =>  kvp.Key, kvp => kvp.Value.Clone());
    }

    public Simulator(JsonElement elem) {
        Now = elem.GetProperty("time").GetDateTime();
        _drones = elem.GetProperty("drones").GetDouble();

        foreach (var prop in elem.GetProperty("materials").EnumerateObject()) {
            _materials.Add(prop.Name, prop.Value.GetInt32());
        }

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
        int i = 0, j = 5;
        foreach (var dormElem in elem.GetProperty("dormitories").EnumerateArray()) {
            var dorm = FacilityBase.FromJson(dormElem, this);
            Dormitories[i++] = dorm as Dormitory;
            AllFacilities[j++] = dorm;
        }
        i = 0;
        foreach (var facElem in elem.GetProperty("facilities").EnumerateArray()) {
            var fac = FacilityBase.FromJson(facElem, this);
            ModifiableFacilities[i++] = fac;
            AllFacilities[j++] = fac;
        }
    }

    public DateTime Now { get; private set; }
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
    void Update(TimeElapsedInfo info) {
        AddDrones((1 + GlobalDronesEffiency) * (info.TimeElapsed / TimeSpan.FromMinutes(6)));
    }
    public void Simulate(TimeSpan span) {
        Resolve();
        var info = new TimeElapsedInfo(Now, Now + span, span);
        foreach (var facility in AllFacilities) {
            facility?.Update(this, info);
        }
        Update(info);
        Now += span;
    }
    public void SimulateUntil(DateTime dateTime, TimeSpan interval) {
        while (Now < dateTime) {
            Simulate(interval);
        }
    }

    internal Dictionary<string, OperatorBase> Operators;
    internal OperatorBase? GetOperator(string name) {
        return Operators.GetValueOrDefault(name);
    }

    internal ControlCenter ControlCenter { get; set; }
    internal Office Office { get; set; }
    internal Reception Reception { get; set; }
    internal Training Training { get; set; }
    internal Crafting Crafting { get; set; }
    internal Dormitory?[] Dormitories = new Dormitory[4];
    internal FacilityBase?[] ModifiableFacilities { get; } = new FacilityBase?[9];
    internal FacilityBase?[] AllFacilities { get; } = new FacilityBase?[18];
    internal IEnumerable<PowerStation> PowerStations
        => ModifiableFacilities.Select(fac => fac as PowerStation).Where(fac => fac != null);
    internal IEnumerable<TradingStation> TradingStations
        => ModifiableFacilities.Select(fac => fac as TradingStation).Where(fac => fac != null);
    internal IEnumerable<ManufacturingStation> ManufacturingStation
        => ModifiableFacilities.Select(fac => fac as ManufacturingStation).Where(fac => fac != null);
    internal IEnumerable<OperatorBase> OperatorsInFacility => AllFacilities.SelectMany(fac => fac?.Operators ?? Enumerable.Empty<OperatorBase>());
    internal bool AddDormitory(Dormitory dorm) {
        for (int i = 0; i < Dormitories.Length; ++i) {
            if (Dormitories[i] == null) {
                Dormitories[i] = dorm;
                AllFacilities[5 + i] = dorm;
            }
        }
        return false;
    }
    internal bool AddFacility(FacilityBase facility) {
        for (int i = 0; i < ModifiableFacilities.Length; ++i) {
            if (ModifiableFacilities[i] == null) {
                ModifiableFacilities[i] = facility;
                AllFacilities[9 + i] = facility;
            }
        }
        return false;
    }


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

    public void DelayAction(Action<Simulator> action, int priority = 100) {
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
