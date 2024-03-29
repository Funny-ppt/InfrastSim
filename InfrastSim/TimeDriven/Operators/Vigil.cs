namespace InfrastSim.TimeDriven.Operators;

internal class Vigil : OperatorBase {
    public override string Name => "伺夜";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1

        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            EfficiencyModifier.SetValue(Name, 25 + simu.Reception.Level * 5);
        }
    }
}
