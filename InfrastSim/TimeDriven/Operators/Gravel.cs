namespace InfrastSim.TimeDriven.Operators;

internal class Gravel : OperatorBase {
    public override string Name => "砾";
    static string[] _groups1 = { "金属工艺" };
    public override string[] Groups => Upgraded >= 1 ? base.Groups : _groups1;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.IsProduceGold()) {
                EfficiencyModifier.SetValue(0.35);
            }
        }
    }
}