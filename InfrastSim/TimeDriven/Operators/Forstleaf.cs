namespace InfrastSim.TimeDriven.Operators;
internal class Frostleaf : OperatorBase {
    public override string Name => "霜叶";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            MoodConsumeRate.SetValue(Name, -0.7);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(Name, 0.3);
            }
        }
    }
}

