namespace InfrastSim.TimeDriven.Operators;
internal class Pallas : OperatorBase {
    public override string Name => "帕拉斯";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            MoodConsumeRate.SetValue(Name, -25);
            manufacturing.Capacity.SetValue(Name, 8);

            if (Upgraded >= 2 && manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(Name, 25);
            }
        }
    }
}
