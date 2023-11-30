namespace InfrastSim.TimeDriven.Operators;
internal class _12F : OperatorBase {
    public override string Name => "12F";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Reception && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.2);
        }
    }
}

