using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public abstract class OperatorBase : ITimeDrivenObject, IJsonSerializable {
    public abstract string Name { get; }
    public virtual string[] Groups => Array.Empty<string>();
    public bool HasGroup(string group) => Groups.Contains(group);
    public FacilityBase? Facility { get; internal set; } = null;

    const double MinMood = 0.0;
    const double MaxMood = 24.0;
    public int Upgraded { get; set; } = 2;
    public double Mood { get; private set; } = 24.0;
    public void SetMood(double mood) {
        Mood = Math.Clamp(mood, MinMood, MaxMood);
    }
    static readonly int[] DefaultThreshold = new int[] { 0 };
    public virtual int[] Thresholds => DefaultThreshold;
    static readonly TimeSpan[] DefaultWorkingTimeThreshold = Array.Empty<TimeSpan>();
    public virtual TimeSpan[] WorkingTimeThresholds => DefaultWorkingTimeThreshold;

    public bool IsTired => Util.Equals(MinMood, Mood);
    public bool IsFullOfEnergy => Util.Equals(MaxMood, Mood);
    public virtual int DormVipPriority => 1;
    public AggregateValue MoodConsumeRate { get; private set; } = new(1);
    public AggregateValue EfficiencyModifier { get; private set; } = new();
    public TimeSpan WorkingTime { get; internal set; } = TimeSpan.Zero;


    public bool LeaveFacility() {
        if (Facility == null) return false;
        return Facility.Remove(this);
    }

    public virtual void Reset() {
        MoodConsumeRate.Clear();
        EfficiencyModifier.Clear();
    }

    public virtual void Resolve(Simulator simu) {
        EfficiencyModifier.MaxValue = double.MaxValue;
        //OnResolve?.Invoke(simu);
    }

    public virtual void QueryInterest(Simulator simu) {
        Debug.Assert(Facility != null);
        if (Facility.IsWorking) {
            var hours = 1000.0;
            if (MoodConsumeRate > 0) {
                foreach (var threshold in Thresholds) {
                    if (Mood - threshold > Util.Epsilon) {
                        hours = Math.Min(hours, (Mood - threshold) / MoodConsumeRate);
                    }
                }
            } else {
                if (MaxMood - Mood > Util.Epsilon) {
                    hours = (Mood - MaxMood) / MoodConsumeRate;
                }
            }
            if (Facility is not Dormitory && !IsTired) {
                foreach (var threshold in WorkingTimeThresholds) {
                    if (threshold > WorkingTime) {
                        hours = Math.Min(hours, (threshold - WorkingTime).TotalHours);
                    }
                }
            }
            simu.SetInterest(this, TimeSpan.FromHours(hours));
        }
    }

    public virtual void Update(Simulator simu, TimeElapsedInfo info) {
        Debug.Assert(Facility != null);
        if (Facility.IsWorking) { // 如果 Update 被调用，则 Facility 必不为null
            SetMood(Mood - MoodConsumeRate * (info.TimeElapsed / TimeSpan.FromHours(1)));
            WorkingTime += info.TimeElapsed;
        }
        if (IsTired) {
            WorkingTime = TimeSpan.Zero;
        }
    }

    //public Action<Simulator>? OnResolve { get; set; }

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
            writer.WriteNumber("working-time-seconds", WorkingTime.TotalSeconds);
            writer.WriteNumber("mood-consume-rate", MoodConsumeRate);
            writer.WriteItem("mood-consume-rate-details", MoodConsumeRate);
            writer.WriteNumber("efficiency", EfficiencyModifier);
            writer.WriteItem("efficiency-details", EfficiencyModifier);
            if (Facility != null) {
                writer.WriteNumber("index", Facility.IndexOf(this));
            }
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
        clone.MoodConsumeRate = new(1); // Important: this must keep with the constructor
        clone.EfficiencyModifier = new();
        return clone;
    }
}