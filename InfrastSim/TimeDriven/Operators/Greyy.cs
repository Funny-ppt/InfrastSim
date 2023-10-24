namespace InfrastSim.TimeDriven.Operators;
internal class Geryy : OperatorBase {
    public override string Name => "格雷伊";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is PowerStation && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.2);
        }

        // TODO: missing skill 2
    }
}

