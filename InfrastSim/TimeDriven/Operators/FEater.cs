namespace InfrastSim.TimeDriven.Operators;

internal class FEater : OperatorBase {
    public override string Name => "食铁兽";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            if (manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 0.35 : 0.3);
            }
        }
    }
}