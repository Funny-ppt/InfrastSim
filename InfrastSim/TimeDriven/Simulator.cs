using RandomEx;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public class Simulator : ISimulator, IJsonSerializable {
    public Simulator() {
        Now = DateTime.UtcNow;
        Random = new XoshiroRandom();
        Facilities[0] = ControlCenter = new();
        Facilities[1] = Office = new();
        Facilities[2] = Reception = new();
        Facilities[3] = Training = new();
        Facilities[4] = Crafting = new();
        for (int i = 5; i < 9; i++) {
            Facilities[i] = new Dormitory();
        }

        Operators = OperatorInstances.Operators.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone());
    }

    public Simulator(in JsonElement elem) {
        if (elem.TryGetProperty("time", out JsonElement timeElem) && timeElem.TryGetDateTime(out var now)) {
            Now = now;
        }

        if (elem.TryGetProperty("random", out JsonElement randomElem)) {
            Random = new XoshiroRandom(randomElem);
        } else {
            Random = new XoshiroRandom();
        }

        if (elem.TryGetProperty("drones", out JsonElement dronesElem)) {
            _drones = dronesElem.GetDouble();
        }

        if (elem.TryGetProperty("refresh", out JsonElement refreshElem)) {
            _refresh = refreshElem.GetDouble();
        }

        if (elem.TryGetProperty("materials", out JsonElement materialsElem)) {
            foreach (var prop in materialsElem.EnumerateObject()) {
                _materials.Add(prop.Name, prop.Value.GetInt32());
            }
        }

        var operators = new Dictionary<string, OperatorBase>();
        if (elem.TryGetProperty("operators", out JsonElement operatorsElem)) {
            foreach (var opElem in operatorsElem.EnumerateArray()) {
                var name = opElem.GetProperty("name").GetString();
                operators[name] = OperatorBase.FromJson(opElem);
            }
        }
        var newOps = OperatorInstances.Operators
            .ExceptBy(operators.Keys, kvp => kvp.Key);
        foreach (var kvp in newOps) {
            operators[kvp.Key] = kvp.Value.Clone();
        }
        Operators = operators.ToFrozenDictionary();

        JsonElement facElem;
        if (elem.TryGetProperty("control-center", out facElem)) {
            ControlCenter = FacilityBase.FromJson(facElem, this) as ControlCenter ?? new ControlCenter();
        }
        if (elem.TryGetProperty("office", out facElem)) {
            Office = FacilityBase.FromJson(facElem, this) as Office ?? new Office();
        }
        if (elem.TryGetProperty("reception", out facElem)) {
            Reception = FacilityBase.FromJson(facElem, this) as Reception ?? new Reception();
        }
        if (elem.TryGetProperty("training", out facElem)) {
            Training = FacilityBase.FromJson(facElem, this) as Training ?? new Training();
        }
        if (elem.TryGetProperty("crafting", out facElem)) {
            Crafting = FacilityBase.FromJson(facElem, this) as Crafting ?? new Crafting();
        }
        ControlCenter ??= new ControlCenter();
        Office ??= new Office();
        Reception ??= new Reception();
        Training ??= new Training();
        Crafting ??= new Crafting();

        int i = 5;
        if (elem.TryGetProperty("dormitories", out JsonElement dormitoriesElem)) {
            foreach (var dormElem in dormitoriesElem.EnumerateArray()) {
                var dorm = FacilityBase.FromJson(dormElem, this);
                Facilities[i++] = dorm;
            }
        }
        if (elem.TryGetProperty("modifiable-facilities", out JsonElement modFacilitiesElem)) {
            foreach (var mfacElem in modFacilitiesElem.EnumerateArray()) {
                var fac = FacilityBase.FromJson(mfacElem, this);
                Facilities[i++] = fac;
            }
        }
    }

    public DateTime Now { get; private set; }
    public XoshiroRandom Random { get; set; }
    ITimeDrivenObject? _interestSource;
    TimeSpan _nextInterest;
    TimeSpan _minSpan = TimeSpan.FromSeconds(2);
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
        foreach (var facility in Facilities) {
            facility?.Reset();
        }
        foreach (var facility in Facilities) {
            facility?.Resolve(this);
        }
        foreach (var actions in _delayActions.Values) {
            actions(this);
        }
        _delayActions.Clear();
    }
    void QueryInterest() {
        _nextInterest = TimeSpan.FromHours(1);
        foreach (var facility in Facilities) {
            facility?.QueryInterest(this);
        }
    }
    void SimulateImpl(TimeSpan span) {
        var info = new TimeElapsedInfo(Now, Now + span, span);
        foreach (var facility in Facilities) {
            facility?.Update(this, info);
        }
        AddDrones(DronesEfficiency * (info.TimeElapsed / TimeSpan.FromMinutes(6)));
        _refresh += OfficeEfficiency * (info.TimeElapsed / TimeSpan.FromHours(12));
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

    internal FrozenDictionary<string, OperatorBase> Operators;
    internal OperatorBase GetOperator(string name) {
        return Operators.GetValueOrDefault(name) ?? throw new KeyNotFoundException($"未知的干员名称 {name}");
    }
    internal OperatorBase? GetOperatorNoThrow(string name) {
        return Operators.GetValueOrDefault(name);
    }

    #region 设施
    public ControlCenter ControlCenter {
        get => (ControlCenter)Facilities[0]!;
        private set => Facilities[0] = value;
    }
    public Office Office {
        get => (Office)Facilities[1]!;
        private set => Facilities[1] = value;
    }
    public Reception Reception {
        get => (Reception)Facilities[2]!;
        private set => Facilities[3] = value;
    }
    public Training Training {
        get => (Training)Facilities[3]!;
        private set => Facilities[3] = value;
    }
    public Crafting Crafting {
        get => (Crafting)Facilities[4]!;
        private set => Facilities[4] = value;
    }
    public ArraySegment<FacilityBase?> Dormitories {
        get => new(Facilities, 5, 4);
    }
    public ArraySegment<FacilityBase?> ModifiableFacilities {
        get => new(Facilities, 9, 9);
    }
    internal FacilityBase?[] Facilities { get; } = new FacilityBase?[18];

    public IEnumerable<PowerStation> PowerStations
        => ModifiableFacilities.Select(fac => fac as PowerStation).Where(fac => fac != null);
    public IEnumerable<TradingStation> TradingStations
        => ModifiableFacilities.Select(fac => fac as TradingStation).Where(fac => fac != null);
    public IEnumerable<ManufacturingStation> ManufacturingStations
        => ModifiableFacilities.Select(fac => fac as ManufacturingStation).Where(fac => fac != null);
    public IEnumerable<OperatorBase> OperatorsInFacility
        => Facilities.SelectMany(fac => fac?.Operators ?? Enumerable.Empty<OperatorBase>());
    public IEnumerable<OperatorBase> WorkingOperators
    => Facilities.SelectMany(fac => fac?.WorkingOperators ?? Enumerable.Empty<OperatorBase>());
    #endregion

    public int TotalPowerConsume =>
        Facilities
        .Where(fac => fac != null && fac is not PowerStation)
        .Sum(fac => fac!.PowerConsumes);

    public int TotalPowerProduce =>
        ModifiableFacilities
        .Where(fac => fac is PowerStation)
        .Sum(fac => -fac!.PowerConsumes);
    public double NextDroneTimeInSeconds =>
        (Math.Ceiling(_drones) - _drones) * 360 / DronesEfficiency;

    double _drones;
    double _refresh;
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

    #region 全局参数
    public AggregateValue SilverVine => GetGlobalValue("木天蓼");
    public AggregateValue Renjianyanhuo => GetGlobalValue("人间烟火");
    public AggregateValue Ganzhixinxi => GetGlobalValue("感知信息");
    public AggregateValue Wushenggongming => GetGlobalValue("无声共鸣");
    public AggregateValue Siweilianhuan => GetGlobalValue("思维链环");
    public AggregateValue Gongchengjiqiren => GetGlobalValue("工程机器人");
    public AggregateValue Jiyisuipian => GetGlobalValue("记忆碎片");
    public AggregateValue Mengjing => GetGlobalValue("梦境");
    public AggregateValue Xiaojie => GetGlobalValue("小节");
    public AggregateValue ExtraGoldProductionLine => GetGlobalValue("虚拟赤金线");
    public AggregateValue ExtraPowerStation => GetGlobalValue("虚拟发电站");
    #endregion

    #region 全局效率
    public AggregateValue GlobalManufacturingEffiency => GetGlobalValue(nameof(GlobalManufacturingEffiency));
    public AggregateValue GlobalTradingEffiency => GetGlobalValue(nameof(GlobalTradingEffiency));
    public double DronesEfficiency => 1 + PowerStations.Sum(power => power.TotalEffiencyModifier);
    public double OfficeEfficiency => 1 + Office.TotalEffiencyModifier;
    public double ManufacturingEfficiency {
        get {
            var count = ManufacturingStations.Count();
            var eff = ManufacturingStations.Sum(fac => fac.TotalEffiencyModifier);
            return count * (1 + GlobalManufacturingEffiency) + eff;
        }
    }
    public double TradingEfficiency {
        get {
            var count = TradingStations.Count();
            var eff = TradingStations.Sum(fac => fac.TotalEffiencyModifier);
            return count * (1 + GlobalTradingEffiency) + eff;
        }
    }
    #endregion

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
        writer.WriteNumber("refresh", _refresh);
        if (detailed) {
            writer.WriteNumber("drones-efficiency", DronesEfficiency);
            writer.WriteNumber("office-efficiency", OfficeEfficiency);
            writer.WriteNumber("manufacturing-efficiency", ManufacturingEfficiency);
            writer.WriteNumber("trading-efficiency", TradingEfficiency);
        }

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

        if (detailed) {
            writer.WritePropertyName("global-props");
            writer.WriteStartObject();
            foreach (var prop in _globalValues) {
                writer.WriteItem(prop.Key, prop.Value, detailed);
            }
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    public Simulator Clone() {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteItemValue(this);
        writer.Flush();
        ms.Position = 0;
        using var doc = JsonDocument.Parse(ms);
        return new Simulator(doc.RootElement);
    }

    /// <summary>
    /// 该方法仅供测试使用：
    /// 如果没有干员访问这两个属性，在Resolve中就不会产生，但Update中制造站和贸易站始终会访问这两个值，
    /// 进而导致出现默认的值，使得序列化、反序列化结果不一致（尽管没有任何影响）
    /// </summary>
    public void EnsurePropExists() {
        var p1 = GlobalManufacturingEffiency;
        var p2 = GlobalTradingEffiency;
    }
}
