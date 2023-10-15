namespace InfrastSim.TimeDriven.Operators;

internal class MrNothing : OperatorBase {
    public override string Name => "乌有";
    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            simu.DelayAction(simu => {
                EffiencyModifier.SetValue(Name, simu.Renjianyanhuo * 0.01);
            });
            simu.Renjianyanhuo.SetValue(Name, simu.Dormitories.Sum(dorm => dorm.Operators.Count()));
        }
    }
}
