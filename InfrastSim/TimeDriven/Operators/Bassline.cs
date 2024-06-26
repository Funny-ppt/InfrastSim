namespace InfrastSim.TimeDriven.Operators;

internal class Bassline : OperatorBase {
    public override string Name => "深律";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Office office && !IsTired) {
            MoodConsumeRate.SetValue(Name, -25);
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 30 : 10);
            if (Upgraded >= 2) {
                simu.Wushenggongming.SetValue(Name, 30); // FIXME: hardcoded
            }
        }
    }
}