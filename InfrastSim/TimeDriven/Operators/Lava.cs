namespace InfrastSim.TimeDriven.Operators;
internal class Lava : OperatorBase {
    public override string Name => "炎熔";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is PowerStation && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.1);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceOriginStone()) {
                EfficiencyModifier.SetValue(Name, 0.35);
            }
        }
    }
}

