using RandomEx;
using System.Collections.Frozen;
using System.Diagnostics;
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
            Now = Round(now);
        }

        if (elem.TryGetProperty("random", out JsonElement randomElem)) {
            Random = new XoshiroRandom(randomElem);
        } else {
            Random = new XoshiroRandom();
        }

        if (elem.TryGetProperty("drones", out JsonElement dronesElem)) {
            _drones = dronesElem.GetInt32();
        }

        if (elem.TryGetProperty("refresh", out JsonElement refreshElem)) {
            _refresh = refreshElem.GetInt32();
        }

        if (elem.TryGetProperty("total-manu-progress", out JsonElement totManuProgElem)) {
            _totalManuProgress = totManuProgElem.GetInt64();
        }

        if (elem.TryGetProperty("total-trad-progress", out JsonElement totTradProgElem)) {
            _totalTradProgress = totTradProgElem.GetInt64();
        }

        if (elem.TryGetProperty("total-office-progress", out JsonElement totOfficeProgElem)) {
            _totalOfficeProgress = totOfficeProgElem.GetInt64();
        }

        if (elem.TryGetProperty("total-drones-progress", out JsonElement totDronesProgElem)) {
            _totalDronesProgress = totDronesProgElem.GetInt64();
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
        } else {
            ControlCenter = new ControlCenter();
        }
        if (elem.TryGetProperty("office", out facElem)) {
            Office = FacilityBase.FromJson(facElem, this) as Office ?? new Office();
        } else {
            Office = new Office();
        }
        if (elem.TryGetProperty("reception", out facElem)) {
            Reception = FacilityBase.FromJson(facElem, this) as Reception ?? new Reception();
        } else {
            Reception = new Reception();
        }
        if (elem.TryGetProperty("training", out facElem)) {
            Training = FacilityBase.FromJson(facElem, this) as Training ?? new Training();
        } else {
            Training = new Training();
        }
        if (elem.TryGetProperty("crafting", out facElem)) {
            Crafting = FacilityBase.FromJson(facElem, this) as Crafting ?? new Crafting();
        } else {
            Crafting = new Crafting();
        }

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
    public static DateTime Round(DateTime original)
        => new(original.Year, original.Month, original.Day, original.Hour, original.Minute, original.Second, DateTimeKind.Utc);

    public XoshiroRandom Random { get; set; }
    ITimeDrivenObject? _interestSource;
    TimeSpan _nextInterest;
    internal void SetInterest(ITimeDrivenObject o, int seconds) {
        if (seconds * TimeSpan.TicksPerSecond < _nextInterest.Ticks) {
            _interestSource = o;
            _nextInterest = TimeSpan.FromSeconds(seconds);
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
        ProduceDrones(DronesEfficiency * info.TimeElapsed.TotalSeconds());
        ProduceRefresh(OfficeEfficiency * info.TimeElapsed.TotalSeconds());
        Now += span;
    }
    public void SimulateUntil(DateTime dateTime) {
        dateTime = Round(dateTime);
        while (Now < dateTime) {
            Resolve();
            QueryInterest();
            var span = dateTime - Now;
            if (_nextInterest < span) span = _nextInterest;
            SimulateImpl(span);
        }
    }

    internal FrozenDictionary<string, OperatorBase> Operators { get; set; }
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
        private set => Facilities[2] = value;
    }
    public Training Training {
        get => (Training)Facilities[3]!;
        private set => Facilities[3] = value;
    }
    public Crafting Crafting {
        get => (Crafting)Facilities[4]!;
        private set => Facilities[4] = value;
    }
    public ArraySegment<FacilityBase?> Dormitories => new(Facilities, 5, 4);
    public ArraySegment<FacilityBase?> ModifiableFacilities => new(Facilities, 9, 9);
    internal FacilityBase?[] Facilities { get; } = new FacilityBase?[18];

    public IEnumerable<PowerStation> PowerStations
        => ModifiableFacilities.Select(fac => fac as PowerStation).Where(fac => fac != null);
    public IEnumerable<TradingStation> TradingStations
        => ModifiableFacilities.Select(fac => fac as TradingStation).Where(fac => fac != null);
    public IEnumerable<ManufacturingStation> ManufacturingStations
        => ModifiableFacilities.Select(fac => fac as ManufacturingStation).Where(fac => fac != null);
    public IEnumerable<OperatorBase> OperatorsInFacility
        => Facilities.SelectMany(fac => fac?.Operators ?? []);
    public IEnumerable<OperatorBase> WorkingOperators
    => Facilities.SelectMany(fac => fac?.WorkingOperators ?? []);
    #endregion

    public int TotalPowerConsume =>
        Facilities
        .Where(fac => fac != null && fac is not PowerStation)
        .Sum(fac => fac!.PowerConsumes);

    public int TotalPowerProduce =>
        ModifiableFacilities
        .Where(fac => fac is PowerStation)
        .Sum(fac => -fac!.PowerConsumes);
    public double NextDroneTimeInSeconds => (TicksHelper.TicksToProduceDrone - _droneProgress) / DronesEfficiency;


    // 这两个参数本来应该由脚本执行器维护，为了方便直接实现到模拟器类
    // 更好的做法是模拟器实现一种扩展方法来管理这些额外的变量的生命周期
    public string SelectedFacilityString { get; set; } = string.Empty;
    public FacilityBase? SelectedFacilityCache { get; set; }

    int _drones;
    int _droneProgress;
    int _refresh;
    int _refreshProgress;
    long _totalManuProgress;
    long _totalTradProgress;
    long _totalOfficeProgress;
    long _totalDronesProgress;


    Dictionary<string, int> _materials = [];
    Dictionary<string, AggregateValue> _globalValues = [];
    SortedDictionary<int, Action<Simulator>> _delayActions = [];

    public void Delay(Action<Simulator> action, int priority = 100) {
        if (_delayActions.ContainsKey(priority)) {
            _delayActions[priority] += action;
        } else {
            _delayActions[priority] = action;
        }
    }
    public int Drones => _drones;
    public void AddDrones(int amount) {
        _drones = Math.Min(200, _drones + amount);
    }
    public void ProduceDrones(int progress) {
        _droneProgress += progress;
        _drones += _droneProgress / TicksHelper.TicksToProduceDrone;
        if (_drones >= 200) {
            _drones = 200;
            _droneProgress = 0;
        } else {
            _droneProgress %= TicksHelper.TicksToProduceDrone;
        }
        _totalDronesProgress += progress;
    }
    public void ProduceRefresh(int progress) {
        _refreshProgress += progress;
        _refresh += _refreshProgress / TicksHelper.TicksToProduceDrone;
        _refreshProgress %= TicksHelper.TicksToProduceDrone;
        _totalOfficeProgress += progress;
    }
    internal void RemoveDrones(int amount) {
        _drones -= amount;
        Debug.Assert(_drones >= 0);
    }
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
    internal void AddManuProgress(int progress) {
        _totalManuProgress += progress;
    }
    internal void AddTradProgress(int progress) {
        _totalTradProgress += progress;
    }

    public AggregateValue GetGlobalValue(string name) {
        if (!_globalValues.ContainsKey(name)) {
            _globalValues.Add(name, new(min: 0));
        }
        return _globalValues[name];
    }

    public IEnumerable<(string, double)> GlobalValues
        => _globalValues.Select(kvp => (kvp.Key, (double)kvp.Value));

    #region 全局参数
    public AggregateValue SilverVine => GetGlobalValue("木天蓼");
    public AggregateValue Renjianyanhuo => GetGlobalValue("人间烟火");
    public AggregateValue Wushujiejing => GetGlobalValue("巫术结晶");
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
    public AggregateValue GlobalManufacturingEfficiency => GetGlobalValue("全局制造站效率");
    public AggregateValue GlobalTradingEfficiency => GetGlobalValue("全局贸易站效率");
    public int DronesEfficiency => 100 + PowerStations.Sum(power => power.TotalEffiencyModifier);
    public int OfficeEfficiency => (Office.WorkingOperators.Any() ? 100 : 0) + Office.TotalEffiencyModifier;
    public int ManufacturingEfficiency {
        get {
            var workingFacilities = ManufacturingStations.Where(fac => fac.Operators.Any());
            var count = workingFacilities.Count();
            var eff = workingFacilities.Sum(fac => fac.TotalEffiencyModifier);
            return count * (100 + GlobalManufacturingEfficiency) + eff;
        }
    }
    public int TradingEfficiency {
        get {
            var workingFacilities = TradingStations.Where(fac => fac.Operators.Any());
            var count = workingFacilities.Count();
            var eff = workingFacilities.Sum(fac => fac.TotalEffiencyModifier);
            return count * (100 + GlobalTradingEfficiency) + eff;
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
        writer.WriteNumber("total-manu-progress", _totalManuProgress);
        writer.WriteNumber("total-trad-progress", _totalTradProgress);
        writer.WriteNumber("total-office-progress", _totalOfficeProgress);
        writer.WriteNumber("total-drones-progress", _totalDronesProgress);
        if (detailed) {
            writer.WriteNumber("drones-efficiency", DronesEfficiency / 100.0);
            writer.WriteNumber("office-efficiency", OfficeEfficiency / 100.0);
            writer.WriteNumber("manufacturing-efficiency", ManufacturingEfficiency / 100.0);
            writer.WriteNumber("trading-efficiency", TradingEfficiency / 100.0);
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
        var p1 = GlobalManufacturingEfficiency;
        var p2 = GlobalTradingEfficiency;
    }
}
