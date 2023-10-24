namespace InfrastSim.TimeDriven.Operators;
internal class Archetto : OperatorBase {
    public override string Name => "空弦";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            var sum = simu.Dormitories.Sum(fac => fac?.Level ?? 0)* (Upgraded >= 2 ? 0.02 : 0.01);
            EfficiencyModifier.SetValue(Name, sum);
        }
    }
}
