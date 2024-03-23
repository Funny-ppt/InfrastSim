namespace InfrastSim.TimeDriven.Operators;
internal class Chestnut : OperatorBase {
    public override string Name => "褐果";
    static string[] _groups = { "标准化", "杜林" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, 15);
            if (Upgraded >= 1 && manufacturing.IsProduceOriginStone()) {
                EfficiencyModifier.AddValue(Name, 30);
            }
        }
    }
}

