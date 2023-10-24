namespace InfrastSim.TimeDriven.Operators;

internal class Spot : OperatorBase {
    public override string Name => "斑点";
    static string[] _groups1 = { "金属工艺" };
    public override string[] Groups => Upgraded >= 1 ? _groups1 : base.Groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1

        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceGold()) {
                EfficiencyModifier.SetValue(0.30);
            }
        }
    }
}