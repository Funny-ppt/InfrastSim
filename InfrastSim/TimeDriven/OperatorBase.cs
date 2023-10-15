namespace InfrastSim.TimeDriven;
internal abstract class OperatorBase : ITimeDrivenObject {
    public abstract string Name { get; }
    public virtual string[] Groups { get; } = Array.Empty<string>();
    public FacilityBase? Facility { get; set; } = null;

    const double MinMood = 0.0;
    const double MaxMood = 24.0;
    public int Upgraded { get; private set; } = 2;
    public double Mood { get; private set; } = 24.0;
    public bool IsTired => Util.Equals(MinMood, Mood);
    public bool IsFullOfEnergy => Util.Equals(MaxMood, Mood);
    public int DormVipPriority { get; set; } = 1;
    public AggregateValue MoodConsumeRate { get; } = new();
    public AggregateValue EffiencyModifier { get; } = new();
    public TimeSpan WorkingTime { get; set; } = TimeSpan.Zero;

    public virtual void Reset() {
        MoodConsumeRate.Clear();
        EffiencyModifier.Clear();
    }

    public virtual void Resolve(TimeDrivenSimulator simu) {
        Reset();
        OnResolve?.Invoke(simu);
    }

    public virtual void Update(TimeDrivenSimulator simu, TimeElapsedInfo info) {
        if (Facility != null && Facility.IsWorking) {
            var newMood = Mood - MoodConsumeRate * (info.TimeElapsed / TimeSpan.FromHours(1));
            Mood = Math.Clamp(newMood, MinMood, MaxMood);
            WorkingTime += info.TimeElapsed;
        }
    }

    public Action<TimeDrivenSimulator>? OnResolve { get; set; }

    // TODO: 添加技能标签，附带解锁精英化等级等信息
}