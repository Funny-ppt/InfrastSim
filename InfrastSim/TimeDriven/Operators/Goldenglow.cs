namespace InfrastSim.TimeDriven.Operators;
internal class Goldenglow : OperatorBase {
    public override string Name => "澄闪";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is PowerStation && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 20 : 10);
        }
    }
}

