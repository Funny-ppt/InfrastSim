namespace InfrastSim.TimeDriven.Operators;
internal class Honeyberry : OperatorBase {
    public override string Name => "蜜莓";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1

        if (Facility is Dormitory dorm && Upgraded >= 2) {
            dorm.SetVipMoodModifier(-0.7);
        }
    }
}
