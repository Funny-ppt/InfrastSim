namespace InfrastSim.TimeDriven.Operators;
internal class Virtuosa : OperatorBase {
    public override string Name => "塑心";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm && !IsTired) {
            simu.Wushenggongming.SetValue(Name, dorm.Operators.Count());

            if (Upgraded >= 2) {
                simu.Delay(simu => {
                    dorm.SetDormMoodModifier(-20 + -simu.Wushenggongming / 5);
                });
            }
        }
    }
}
