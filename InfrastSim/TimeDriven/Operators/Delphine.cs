namespace InfrastSim.TimeDriven.Operators;
internal class Delphine : OperatorBase {
    public override string Name => "戴菲恩";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired && Upgraded >= 2) {
            var ops = simu.OperatorsInFacility.Where(op => op.Facility is TradingStation && op.Groups.Contains("格拉斯哥帮"));
            foreach (var op in ops) {
                op.EfficiencyModifier.SetValue(Name, 10);
            }
        }
    }
}
