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
        if (Operators.Count() == AcceptOperatorNums || op.Facility != null) {
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
    public bool Upgrade(int level) {
        throw new NotImplementedException();
    }
    public bool Downgrade(int level) {
        throw new NotImplementedException();
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
}
