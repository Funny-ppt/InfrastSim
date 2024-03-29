namespace InfrastSim.TimeDriven;
public class Dormitory : FacilityBase {
    public override FacilityType Type => FacilityType.Dormitory;
    public override int PowerConsumes => Level switch {
        1 => 10,
        2 => 20,
        3 => 30,
        4 => 45,
        5 => 65,
        _ => 0,
    };

    public override bool IsWorking => true; // force operators to calculate mood change
    public override int AcceptOperatorNums => 5;

    public override int MoodConsumeModifier {
        get {
            return -(100 + 150 + 10 * Level); // 宿舍等级提供 1.6-2 每小时心情回复，其中100为抵消角色基础消耗100
        }
    }


    public OperatorBase? Vip { get; private set; }
    public int VipMoodModifier { get; private set; }
    public int DormMoodModifier { get; private set; }
    public void SetVipMoodModifier(int value) {
        VipMoodModifier = Math.Min(VipMoodModifier, value);
    }
    public void SetDormMoodModifier(int value) {
        DormMoodModifier = Math.Min(DormMoodModifier, value);
    }
    public override int EffiencyModifier => 0;
    public int Atmosphere => Level * 1000;

    public override void Reset() {
        base.Reset();

        DormMoodModifier = 0;
        VipMoodModifier = 0;
    }

    class VipPriorityComparer : IComparer<OperatorBase> {
        public int Compare(OperatorBase? x, OperatorBase? y) {
            if (x.DormVipPriority != y.DormVipPriority) {
                return y.DormVipPriority - x.DormVipPriority;
            }
            if (x.MoodTicks != y.MoodTicks) {
                return x.MoodTicks - y.MoodTicks;
            }
            return x.WorkingTime < y.WorkingTime ? -1 : 1;
        }
    }
    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (VipMoodModifier == 0 || Vip != null && (Vip.Facility != this || Vip.IsFullOfEnergy)) {
            Vip = null;
        }
        if (VipMoodModifier < 0 && Vip == null) {
            Vip = Operators
                .OrderBy(op => op, new VipPriorityComparer())
                .Where(op => !op.IsFullOfEnergy)
                .FirstOrDefault();
        }
        Vip?.MoodConsumeRate.SetValue("dorm-vip", VipMoodModifier);

        simu.Delay((simu) => {
            foreach (var op in Operators) {
                op.MoodConsumeRate.SetValue("dorm-atmosphere", (int)(-0.04 * Atmosphere));
                op.MoodConsumeRate.SetValue("dorm-extra", DormMoodModifier);
                op.MoodConsumeRate.Disable("control-center");
                op.MoodConsumeRate.Disable("control-center-extra");
            }
        }, Priority.Facility);
    }
}
