namespace InfrastSim.TimeDriven.Operators;
internal class Podenco : OperatorBase {
    public override string Name => "波登可";
    public override int DormVipPriority => 0;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(Upgraded >= 1 ? -0.65 : -0.55);

            if (Upgraded >= 1) {
                dorm.SetDormMoodModifier(-0.15);
            }
        }
    }
}
