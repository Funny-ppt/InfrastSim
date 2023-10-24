namespace InfrastSim.TimeDriven.Operators;
internal class Amiya : OperatorBase {
    public override string Name => "阿米娅";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            simu.GlobalTradingEffiency.SetIfGreater(0.07);
        }
        if (Facility is Dormitory dorm && Upgraded >= 2) {
            dorm.SetDormMoodModifier(-0.15);
        }
    }
}
