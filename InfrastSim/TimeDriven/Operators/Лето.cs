namespace InfrastSim.TimeDriven.Operators;
internal class Лето : OperatorBase {
    public override string Name => "烈夏";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1

        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 2) {
            if (manufacturing.IsProduceCombatRecord()
                && simu.IsOpInFacility("古米", FacilityType.Trading)) {

                EfficiencyModifier.SetValue(Name, 0.3);
            }
        }
    }
}

