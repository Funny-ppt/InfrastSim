namespace InfrastSim.TimeDriven.Operators;
internal class Liskarm : OperatorBase {
    public override string Name => "雷蛇";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is PowerStation power && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 20 : 15);
        }
    }
}

