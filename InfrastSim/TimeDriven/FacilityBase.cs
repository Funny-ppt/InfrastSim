using InfrastSim.TimeDriven.Operators;
using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public abstract class FacilityBase : ITimeDrivenObject, IJsonSerializable {
    public abstract FacilityType Type { get; }
    public int Level { get; protected set; }
    public virtual int PowerConsumes => Level switch {
        1 => 10,
        2 => 30,
        3 => 60,
        _ => 0,
    }; // 大多数基建建筑满足 10-30-60的规律，节省重复代码量。

    internal OperatorBase?[] _operators = new OperatorBase[5];
    public abstract int AcceptOperatorNums { get; }

    public IEnumerable<OperatorBase> Operators
        => _operators.Take(AcceptOperatorNums).Where(op => op != null);

    public int IndexOf(OperatorBase? op) {
        return Array.IndexOf(_operators, op);
    }
    public virtual bool IsWorking => Operators.Any();
    public IEnumerable<OperatorBase> WorkingOperators => Operators.Where(op => !op.IsTired);
    public int WorkingOperatorsCount => WorkingOperators.Count();

    internal void AssignAt(OperatorBase op, int index) {
        _operators[index] = op;
        op.Facility = this;
    }
    internal void RemoveAt(int index) {
        var op = _operators[index];
        if (op != null) {
            op.Facility = null;
        }
        _operators[index] = null;
    }

    public bool Assign(OperatorBase? op) {
        if (op == null || Operators.Count() == AcceptOperatorNums || op.Facility == this) {
            return false;
        }
        op.LeaveFacility();
        op.WorkingTime = TimeSpan.Zero;
        foreach (var op1 in Operators) {
            op1.WorkingTime += new TimeSpan(1);
        }
        AssignAt(op, IndexOf(null));

        // 游戏中，菲亚会在入住瞬间结算技能。
        if (op is Fiammetta fiammetta && this is Dormitory dorm) {
            fiammetta.TryExchange(dorm);
        }
        return true;
    }
    public void AssignMany(IEnumerable<OperatorBase> ops) {
        var iter = ops.GetEnumerator();
        for (int i = 0; i < AcceptOperatorNums; ++i) {
            var cur = iter.MoveNext() ? iter.Current : null;
            if (cur != _operators[i]) {
                RemoveAt(i);
                cur?.LeaveFacility();
                Assign(cur);
            }
        }
    }
    public bool Remove(OperatorBase? op) {
        if (op == null || op.Facility != this) {
            return false;
        }
        var index = IndexOf(op);
        if (index == -1) return false;
        _operators[index] = null;
        op.Facility = null;
        return true;
    }
    public void RemoveAll() {
        for (int i = 0; i < AcceptOperatorNums; i++) {
            RemoveAt(i);
        }
    }

    public void SetLevel(int level) {
        if (level != Level) {
            RemoveAll();
            Level = level;
        }
    }

    public abstract int MoodConsumeModifier { get; }
    public abstract int EffiencyModifier { get; }
    public int TotalEffiencyModifier => WorkingOperators.Sum(op => op.EfficiencyModifier) + EffiencyModifier;

    public virtual void Reset() {
        foreach (var op in Operators) {
            op.Reset();
        }
    }
    /// <summary>
    /// 该方法应该在派生类的方法开始前被调用。
    /// 默认会调用所有内部干员的Resolve方法，然后处理基建的心情调整值。
    /// </summary>
    public virtual void Resolve(Simulator simu) {
        foreach (var op in Operators) {
            op.Resolve(simu);
        }

        foreach (var op in Operators) {
            op.MoodConsumeRate.SetValue("control-center", simu.ControlCenter.MoodConsumeModifier);
            if (Type != FacilityType.ControlCenter) {
                op.MoodConsumeRate.SetValue("facility", MoodConsumeModifier);
            }
        }
    }

    public virtual void QueryInterest(Simulator simu) {
        foreach (var op in Operators) {
            op.QueryInterest(simu);
        }
    }

    /// <summary>
    /// 该方法应该在派生类的方法结束前被调用。
    /// 默认会调用所有内部干员的Update方法。
    /// </summary>
    public virtual void Update(Simulator simu, TimeElapsedInfo info) {
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
        for (int i = 0; i < AcceptOperatorNums; ++i) {
            var op = _operators[i];
            if (op != null) {
                if (detailed) {
                    writer.WriteItemValue(op, detailed);
                } else {
                    writer.WriteStringValue(op.Name);
                }
            } else {
                writer.WriteNullValue();
            }
        }
        writer.WriteEndArray();

        WriteDerivedContent(writer, detailed);

        if (detailed) {
            writer.WriteNumber("base-efficiency", (100 + EffiencyModifier) / 100.0);
            writer.WriteNumber("operators-efficiency", WorkingOperators.Sum(op => op.EfficiencyModifier) / 100.0);
        }

        writer.WriteEndObject();
    }
    public static FacilityBase? FromJson(in JsonElement elem, Simulator simu) {
        if (elem.ValueKind == JsonValueKind.Null) return null;

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
            int i = -1;
            foreach (var op_elem in operators.EnumerateArray()) {
                i += 1;
                var name = op_elem.ValueKind switch {
                    JsonValueKind.Object => op_elem.GetProperty("name").GetString(),
                    JsonValueKind.String => op_elem.GetString(),
                    _ => null
                };
                if (name == null) {
                    continue;
                }
                var op = simu.GetOperator(name);
                if (op.Facility != null) {
                    throw new JsonException($"{name} 干员被指派到两个不同的设施");
                }
                fac.AssignAt(op, i);
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
