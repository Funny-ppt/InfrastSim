using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
public abstract class OperatorBase : ITimeDrivenObject, IJsonSerializable {
    public abstract string Name { get; }
    public virtual string[] Groups => [];
    public bool HasGroup(string group) =>
           Groups.Contains(group)
        || OperatorGroups.Groups.GetValueOrDefault(group)?.Contains(Name) == true;
    public FacilityBase? Facility { get; internal set; } = null;

    public const double MaxMood = 24.0;
    public const double MinMood = 0.0;
    public const int MaxTicks = 8640000;
    public const int TiredTicks = 50;
    public const int MinTicks = 0;

    static int MoodToTicks(double mood) {
        return (int)(mood / MaxMood * MaxTicks);
    }

    public int Upgraded { get; set; } = 2;
    public double Mood => (double)MoodTicks / MaxTicks * MaxMood;
    public int MoodTicks { get; private set; } = MaxTicks;
    public void SetMood(double mood) {
        MoodTicks = MoodToTicks(Math.Clamp(mood, MinMood, MaxMood));
    }
    public void SetMood(int mood) {
        MoodTicks = Math.Clamp(mood, MinTicks, MaxTicks);
    }
    static readonly int[] DefaultThreshold = [0];
    public virtual int[] Thresholds => DefaultThreshold;
    static readonly TimeSpan[] DefaultWorkingTimeThreshold = [];
    public virtual TimeSpan[] WorkingTimeThresholds => DefaultWorkingTimeThreshold;

    public bool IsTired => MoodTicks <= TiredTicks;
    public bool IsExausted => MoodTicks == MinTicks;
    public bool IsFullOfEnergy => MoodTicks == MaxTicks;
    public virtual int DormVipPriority => 1;
    public AggregateValue MoodConsumeRate { get; private set; } = new(100, displayAsPercentage: true);
    public AggregateValue EfficiencyModifier { get; private set; } = new(displayAsPercentage: true);
    public TimeSpan WorkingTime { get; internal set; } = TimeSpan.Zero;

    public bool LeaveFacility() {
        if (Facility == null) return false;
        return Facility.Remove(this);
    }

    public virtual void Reset() {
        MoodConsumeRate.Clear();
        EfficiencyModifier.Clear();
        EfficiencyModifier.MaxValue = int.MaxValue;
    }

    public virtual void Resolve(Simulator simu) {
    }

    public virtual void QueryInterest(Simulator simu) {
        Debug.Assert(Facility != null);
        if (Facility.IsWorking) {
            var seconds = MaxTicks / 100; // 距离最近检查点的秒数
            if (MoodConsumeRate > 0) {
                //foreach (var threshold in Thresholds) {
                //    if (Mood - threshold > Util.Epsilon) {
                //        hours = Math.Min(hours, (Mood - threshold) / MoodConsumeRate);
                //    }
                //}
                // PS: 方舟自身的代码似乎也不检查心情过界技能的触发 @孤独的人
                // 尝试保持行为和方舟一致。

                if (MoodTicks > TiredTicks) {
                    seconds = (MoodTicks - TiredTicks + MoodConsumeRate - 1) / MoodConsumeRate;
                }
            } else {
                if (MaxTicks > MoodTicks) {
                    seconds = (MoodTicks - MaxTicks - MoodConsumeRate + 1) / MoodConsumeRate;
                }
            }
            if (Facility is not Dormitory && !IsTired) {
                foreach (var threshold in WorkingTimeThresholds) {
                    if (threshold > WorkingTime) {
                        seconds = Math.Min(seconds, threshold.TotalSeconds() - WorkingTime.TotalSeconds());
                        break;
                    }
                }
            }
            simu.SetInterest(this, seconds);
            // TEST REQUIRED: 该方法未经测试，可能有重大bug
        }
    }

    public virtual void Update(Simulator simu, TimeElapsedInfo info) {
        Debug.Assert(Facility != null); // 如果 Update 被调用，则 Facility 必不为null
        if (Facility.IsWorking) {
            int consumes = info.TimeElapsed.TotalSeconds() * MoodConsumeRate;
            MoodTicks = Math.Clamp(MoodTicks - consumes, TiredTicks, MaxTicks);
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
        writer.WriteNumber("mood-ticks", MoodTicks);
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
    public static OperatorBase? FromJson(in JsonElement elem) {
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
            op.SetMood(MoodToTicks(mood.GetDouble()));
        }
        if (elem.TryGetProperty("mood-ticks", out var moodTicks)) {
            op.SetMood(moodTicks.GetInt32());
        }
        if (elem.TryGetProperty("working-time", out var workingTime)) {
            op.WorkingTime = new TimeSpan(workingTime.GetInt64());
        }
        return op;
    }
    public OperatorBase Clone() {
        var clone = (OperatorBase)MemberwiseClone();
        clone.Facility = null;
        clone.MoodConsumeRate = new(100, displayAsPercentage: true); // Important: this must keep with the constructor
        clone.EfficiencyModifier = new(displayAsPercentage: true);
        return clone;
    }
}