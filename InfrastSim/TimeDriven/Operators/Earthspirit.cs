namespace InfrastSim.TimeDriven.Operators;
internal class Earthspirit : OperatorBase {
    public override string Name => "地灵";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Office office && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 1 ? 45 : 30);
            if (Upgraded >= 1) {
                MoodConsumeRate.SetValue(Name, 200);
            }
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceOriginStone()) {
                EfficiencyModifier.SetValue(Name, 35);
            }
        }
    }
}

