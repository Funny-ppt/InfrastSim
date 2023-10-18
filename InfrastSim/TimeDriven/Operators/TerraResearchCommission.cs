namespace InfrastSim.TimeDriven.Operators;
internal class TerraResearchCommission : OperatorBase {
    public override string Name => "泰拉大陆调查团";
    static string[] _groups = { "怪物猎人小队" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            manufacturing.Capacity.SetValue(Name, 8);
            EfficiencyModifier.SetValue(Name, 0.05 + simu.SilverVine * 0.01);
        }
        if (Facility is TradingStation trading && !IsTired) {
            trading.Capacity.SetValue(Name, 2);
            EfficiencyModifier.SetValue(Name, 0.05 + simu.SilverVine * 0.03);
        }
    }
}
