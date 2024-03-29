namespace InfrastSim.TimeDriven.Operators;
internal class ExecutorTheExFoedere : OperatorBase {
    public override string Name => "圣约送葬人";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            if (manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 30 : 20);
                if (Upgraded >= 2) {
                    manufacturing.Capacity.SetValue(Name, 4);
                }
            }
        }
    }
}
