using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace InfrastSim.TimeDriven;
internal abstract class FacilityBase : ITimeDrivenObject {
    public abstract FacilityType Type { get; }
    public int Level { get; internal set; }
    public virtual int PowerConsumes => Level switch {
        1 => 10,
        2 => 30,
        3 => 60,
        _ => 0,
    }; // 大多数基建建筑满足 10-30-60的规律，节省重复代码量。

    OperatorBase?[] _operators = new OperatorBase[5];
    public abstract int AcceptOperatorNums { get; }
    public IEnumerable<OperatorBase> Operators
        => _operators.Take(AcceptOperatorNums).Where(op => op != null);
    public virtual bool IsWorking => Operators.Any();
    public IEnumerable<OperatorBase> WorkingOperators => Operators.Where(op => !op.IsTired);
    public int WorkingOperatorsCount => WorkingOperators.Count();

    public bool Assign(OperatorBase op) {
        if (op == null || Operators.Count() == AcceptOperatorNums || op.Facility != null) {
            return false;
        }
        var index = Array.IndexOf(_operators, null);
        _operators[index] = op;
        op.Facility = this;
        op.WorkingTime = TimeSpan.Zero;
        return true;
    }
    public bool Remove(OperatorBase op) {
        var index = Array.IndexOf(_operators, op);
        if (index == -1) return false;
        _operators[index] = null;
        op.Facility = null;
        return true;
    }
    public void SetLevel(int level) {
        foreach (var op in Operators) {
            Remove(op);
        }
        Level = level;
    }

    public abstract double MoodConsumeModifier { get; }
    public abstract double EffiencyModifier { get; }
    public double TotalEffiencyModifier => WorkingOperators.Sum(op => op.EffiencyModifier) + EffiencyModifier;

    public virtual void Reset() {
        foreach (var op in Operators) {
            op.Reset();
        }
    }
    /// <summary>
    /// 该方法应该在派生类的方法开始前被调用。
    /// 默认会调用所有内部干员的Resolve方法，然后处理基建的心情调整值。
    /// </summary>
    public virtual void Resolve(TimeDrivenSimulator simu) {
        foreach (var op in Operators) {
            op.Resolve(simu);
        }

        foreach (var op in Operators) {
            op.MoodConsumeRate.SetValue("control-center", simu.ControlCenter.EffiencyModifier);
            if (Type != FacilityType.ControlCenter) {
                op.MoodConsumeRate.SetValue("facility", MoodConsumeModifier);
            }
        }
    }

    /// <summary>
    /// 该方法应该在派生类的方法结束前被调用。
    /// 默认会调用所有内部干员的Update方法。
    /// </summary>
    public virtual void Update(TimeDrivenSimulator simu, TimeElapsedInfo info) {
        foreach (var op in Operators) {
            op.Update(simu, info);
        }
    }

    public string ToJson(bool detailed = false) {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        ToJson(writer, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray()) ?? string.Empty;
    }
    public void ToJson(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteStartObject();
        writer.WriteString("type", Type.ToString());
        writer.WriteNumber("level", Level);

        writer.WritePropertyName("operators");
        writer.WriteStartArray();
        foreach(var op in Operators) {
            op.ToJson(writer, detailed);
        }
        writer.WriteEndArray();

        WriteDerivedContent(writer, detailed);

        if (detailed) {
            // TODO
        }

        writer.WriteEndObject();
    }
    public static FacilityBase? FromJson(JsonElement elem) {
        if (!elem.TryGetProperty("type", out var type)) {
            return null;
        }
        FacilityBase? fac = type.GetString() switch {
            "ControlCenter" => new ControlCenter(),
            "Office" => new Office(),
            "Dormitory" => new Dormitory(),
            "Crafting" => new Crafting(),
            "Training" => new Training(),
            "Trading" => new TradingStation(),
            "Manufacturing" => new ManufacturingStation(),
            "Power" => new PowerStation(),
            "Reception" => new Reception(),
            _ => null
        };
        if (fac == null) return null;


        if (elem.TryGetProperty("level", out var level)) {
            fac.Level = level.GetInt32();
        }
        if (elem.TryGetProperty("operators", out var operators) && operators.ValueKind == JsonValueKind.Array) {
            foreach (var op_elem in operators.EnumerateArray()) {
                fac.Assign(OperatorBase.FromJson(op_elem));
            }
        }
        fac.ReadDerivedContent(elem);
        return fac;
    }
    protected virtual void WriteDerivedContent(Utf8JsonWriter writer, bool detailed = false) {
    }
    protected virtual void ReadDerivedContent(JsonElement elem) {
    }
}
