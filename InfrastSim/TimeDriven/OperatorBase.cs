using System.Text;
using System.Text.Json;

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
    public virtual int DormVipPriority => 1;
    public AggregateValue MoodConsumeRate { get; private set; } = new();
    public AggregateValue EffiencyModifier { get; private set; } = new();
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

    public string ToJson(bool detailed = false) {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        ToJson(writer, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray()) ?? string.Empty;
    }
    public void ToJson(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteStartObject();
        writer.WriteString("name", Name);
        writer.WriteNumber("upgraded", Upgraded);
        writer.WriteNumber("mood", Mood);
        writer.WriteNumber("working-time", WorkingTime.Ticks);

        if (detailed) {
            // TODO
        }

        writer.WriteEndObject();
    }
    public static OperatorBase? FromJson(JsonElement elem) {
        if (!elem.TryGetProperty("name", out var name)) {
            return null;
        }

        var op = OperatorInstances.Operators.GetValueOrDefault(name.GetString());
        if (op == null) return null;

        op = op.Clone();
        if (elem.TryGetProperty("upgraded", out var upgraded)) {
            op.Upgraded = upgraded.GetInt32();
        }
        if (elem.TryGetProperty("mood", out var mood)) {
            op.Mood = Math.Clamp(mood.GetDouble(), MinMood, MaxMood);
        }
        if (elem.TryGetProperty("working-time", out var workingTime)) {
            op.WorkingTime = new TimeSpan(workingTime.GetInt64());
        }
        return op;
    }
    public OperatorBase Clone() {
        var clone = (OperatorBase)MemberwiseClone();
        clone.Facility = null;
        clone.MoodConsumeRate = new();
        clone.EffiencyModifier = new();
        return clone;
    }
}