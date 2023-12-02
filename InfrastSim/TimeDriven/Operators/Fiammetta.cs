namespace InfrastSim.TimeDriven.Operators;
internal class Fiammetta : OperatorBase {
    public override string Name => "菲亚梅塔";
    public override int DormVipPriority => -1;

    public void TryExchange(Dormitory dorm) {
        if (IsFullOfEnergy) {
            var arr = dorm.Operators.OrderBy(op => op.WorkingTime).ToArray();
            var index = Array.IndexOf(arr, this);
            for (int i = index + 1; i < arr.Length; i++) {
                var op = arr[i];
                if (!op.IsFullOfEnergy) {
                    SetMood(op.MoodTicks);
                    op.SetMood(MaxTicks);
                    break;
                }
            }
        }
    }
    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            MoodConsumeRate.MinValue = -2;
            MoodConsumeRate.MaxValue = -2;
            TryExchange(dorm);
        } else {
            MoodConsumeRate.MinValue = double.MinValue;
            MoodConsumeRate.MaxValue = double.MaxValue;
        }
    }
}

