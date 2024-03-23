namespace InfrastSim.TimeDriven.Operators;
internal class Nightingale : OperatorBase {
    public override string Name => "夜莺";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetDormMoodModifier(Upgraded >= 2 ? -20 : -10);
        }
    }
}

