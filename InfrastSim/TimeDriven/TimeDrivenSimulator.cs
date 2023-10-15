using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public class TimeDrivenSimulator : ISimulator {
    public TimeDrivenSimulator() {
        Now = DateTime.Now;
        AllFacilities[0] = ControlCenter = new();
        AllFacilities[1] = Office = new();
        AllFacilities[2] = Reception = new();
        AllFacilities[3] = Training = new();
        AllFacilities[4] = Crafting = new();
    }

    public TimeDrivenSimulator(JsonElement elem) {
        Now = elem.GetProperty("time").GetDateTime();
        _drones = elem.GetProperty("drones").GetDouble();

        foreach (var prop in elem.GetProperty("materials").EnumerateObject()) {
            _materials.Add(prop.Name, prop.Value.GetInt32());
        }

        AllFacilities[0] = ControlCenter = FacilityBase.FromJson(elem.GetProperty("control-center")) as ControlCenter;
        AllFacilities[1] = Office = FacilityBase.FromJson(elem.GetProperty("office")) as Office;
        AllFacilities[2] = Reception = FacilityBase.FromJson(elem.GetProperty("reception")) as Reception;
        AllFacilities[3] = Training = FacilityBase.FromJson(elem.GetProperty("training")) as Training;
        AllFacilities[4] = Crafting = FacilityBase.FromJson(elem.GetProperty("crafting")) as Crafting;
        int i = 0, j = 5;
        foreach (var dormElem in elem.GetProperty("dormitories").EnumerateArray()) {
            var dorm = FacilityBase.FromJson(dormElem);
            Dormitories[i++] = dorm as Dormitory;
            AllFacilities[j++] = dorm;
        }
        i = 0;
        foreach (var facElem in elem.GetProperty("facilities").EnumerateArray()) {
            var fac = FacilityBase.FromJson(facElem);
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
            facility.Reset();
        }
        foreach (var facility in AllFacilities) {
            facility.Resolve(this);
        }
        _delayActions?.Invoke(this);
        _delayActions = null;
    }
    void Update(TimeElapsedInfo info) {
        AddDrones((1 + GlobalDronesEffiency) * (info.TimeElapsed / TimeSpan.FromMinutes(6)));
    }
    public void Simulate(TimeSpan span) {
        Resolve();
        var info = new TimeElapsedInfo(Now, Now + span, span);
        foreach (var facility in AllFacilities) {
            facility.Update(this, info);
        }
        Update(info);
        Now += span;
    }
    public void SimulateUntil(DateTime dateTime, TimeSpan interval) {
        while (Now < dateTime) {
            Simulate(interval);
        }
    }


    internal ControlCenter ControlCenter;
    internal Office Office;
    internal Reception Reception;
    internal Training Training;
    internal Crafting Crafting;
    internal Dormitory?[] Dormitories = new Dormitory[4];
    internal FacilityBase?[] ModifiableFacilities { get; } = new FacilityBase?[9];
    internal FacilityBase?[] AllFacilities { get; } = new FacilityBase?[18];
    internal IEnumerable<PowerStation> PowerStations
        => ModifiableFacilities.Select(fac => fac as PowerStation).Where(fac => fac != null);
    internal IEnumerable<TradingStation> TradingStations
        => ModifiableFacilities.Select(fac => fac as TradingStation).Where(fac => fac != null);
    internal IEnumerable<ManufacturingStation> ManufacturingStation
        => ModifiableFacilities.Select(fac => fac as ManufacturingStation).Where(fac => fac != null);
    internal IEnumerable<OperatorBase> Operators => AllFacilities.SelectMany(fac => fac?.Operators ?? Enumerable.Empty<OperatorBase>());
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
    void ConsumeMaterial(Material mat) {
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

        writer.WritePropertyName("materials");
        writer.WriteStartObject();
        foreach (var kvp in _materials) {
            writer.WriteNumber(kvp.Key, kvp.Value);
        }
        writer.WriteEndObject();

        writer.WritePropertyName("control-center");
        ControlCenter.ToJson(writer, detailed);
        writer.WritePropertyName("office");
        Office.ToJson(writer, detailed);
        writer.WritePropertyName("reception");
        Reception.ToJson(writer, detailed);
        writer.WritePropertyName("training");
        Training.ToJson(writer, detailed);
        writer.WritePropertyName("crafting");
        Crafting.ToJson(writer, detailed);

        writer.WritePropertyName("dormitories");
        writer.WriteStartArray();
        foreach (var fac in Dormitories) {
            if (fac == null) {
                writer.WriteNullValue();
            } else {
                fac.ToJson(writer, detailed);
            }
        }
        writer.WriteEndArray();

        writer.WritePropertyName("modifiable-facilities");
        writer.WriteStartArray();
        foreach (var fac in ModifiableFacilities) {
            if (fac == null) {
                writer.WriteNullValue();
            } else {
                fac.ToJson(writer, detailed);
            }
        }
        writer.WriteEndArray();

        if (detailed) {
            // TODO
        }

        writer.WriteEndObject();
    }
}
