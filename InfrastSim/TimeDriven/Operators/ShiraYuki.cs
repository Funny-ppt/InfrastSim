namespace InfrastSim.TimeDriven.Operators;
internal class ShiraYuki : OperatorBase {
    public override string Name => "白雪";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Reception && !IsTired) {
            EfficiencyModifier.SetValue(20);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(30);
            }
        }
    }
}

