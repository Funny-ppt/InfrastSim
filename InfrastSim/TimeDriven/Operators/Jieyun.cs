namespace InfrastSim.TimeDriven.Operators;

internal class Jieyun : OperatorBase {
    public override string Name => "截云";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            simu.Delay(simu => {
                simu.Wushujiejing.SetValue(Name, simu.Renjianyanhuo / 5);
            }, Priority.PropConversion);
            simu.Delay(simu => {
                EfficiencyModifier.SetValue(Name, simu.Wushujiejing * (Upgraded >= 2 ? 2 : 1));
            });
        }
    }
}