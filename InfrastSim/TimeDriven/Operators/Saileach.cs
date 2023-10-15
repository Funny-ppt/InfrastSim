namespace InfrastSim.TimeDriven.Operators;
internal class Saileach : OperatorBase {
    public Saileach() { DormVipPriority = 0; }
    public override string Name => "琴柳";

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.GetVip()?.MoodConsumeRate.SetIfLesser("dorm-vip", -0.7);
        }
    }
}
