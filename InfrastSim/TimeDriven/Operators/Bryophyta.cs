namespace InfrastSim.TimeDriven.Operators;

internal class Bryophyta : OperatorBase {
    public override string Name => "苍苔";
    static string[] _groups = { "金属工艺" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing) {
            if (manufacturing.IsProduceGold()) {
                EfficiencyModifier.SetValue(Name, 0.30);
            }
            if (Upgraded >= 2) {
                EfficiencyModifier.AddValue(Name, 0.05 * manufacturing.GroupMemberCount("金属工艺"));
            }
        }
    }
}