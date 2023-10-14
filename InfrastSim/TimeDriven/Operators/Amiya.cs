namespace InfrastSim.TimeDriven.Operators;
internal class Amiya : OperatorBase {
    public override string Name => "阿米娅";

    public override void Reset(TimeDrivenSimulator simu) {
        base.Reset(simu);

        simu.GlobalTradingEffiency.SetValue(0);
        if (Facility is Dormitory dorm) {
            dorm.DormMoodModifier.SetValue(0);
        }
    }

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            simu.GlobalTradingEffiency.SetIfGreater(0.07);
        }
        if (Facility is Dormitory dorm) {
            dorm.DormMoodModifier.SetIfLesser(-0.15);
        }
    }
}
