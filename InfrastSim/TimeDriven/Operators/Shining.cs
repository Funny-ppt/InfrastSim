namespace InfrastSim.TimeDriven.Operators;
internal class Shining : OperatorBase {
    public override string Name => "闪灵";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(Upgraded >= 2 ? -75 : -55);
        }
    }
}

