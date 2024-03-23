namespace InfrastSim.TimeDriven.Operators;
internal class Rangers : OperatorBase {
    public override string Name => "巡林者";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Office office && !IsTired) {
            EfficiencyModifier.SetValue(Name, 20);
        }

        // TODO: missing skill 2
    }
}

