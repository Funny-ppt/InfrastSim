namespace InfrastSim.TimeDriven.Operators;
internal class Amiya : OperatorBase {
    public override string Name => "阿米娅";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            simu.GlobalTradingEfficiency.SetIfGreater(7);
        }
        if (Facility is Dormitory dorm && Upgraded >= 2) {
            dorm.SetDormMoodModifier(-15);
        }
    }
}
