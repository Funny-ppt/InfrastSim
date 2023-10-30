namespace InfrastSim.TimeDriven.Operators;

internal class Conviction : OperatorBase {
    public override string Name => "断罪者";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-0.7);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(Name, 0.35);
            }
        }
    }
}