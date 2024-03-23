namespace InfrastSim.TimeDriven.Operators;

internal class Mulberry : OperatorBase {
    public override string Name => "桑葚";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Office office && !IsTired) {
            MoodConsumeRate.SetValue(Name, -25);
            EfficiencyModifier.SetValue(Name, Upgraded >= 1 ? 20 : 10);
            if (Upgraded >= 2) {
                simu.Renjianyanhuo.SetValue(Name, 20); // FIXME: hardcoded
            }
        }
    }
}