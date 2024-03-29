namespace InfrastSim.TimeDriven.Operators;
internal class Ebenholz : OperatorBase {
    public override string Name => "黑键";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            var sum = simu.Dormitories.Sum(fac => fac?.Operators.Count() ?? 0);
            simu.Ganzhixinxi.SetValue(Name, sum);

            simu.Delay(simu => {
                simu.Wushenggongming.SetValue(Name, simu.Ganzhixinxi);
            }, Priority.PropConversion);

            simu.Delay(simu => {
                EfficiencyModifier.SetValue(Name, simu.Wushenggongming / (Upgraded >= 2 ? 2 : 4));
            });
        }
    }
}
