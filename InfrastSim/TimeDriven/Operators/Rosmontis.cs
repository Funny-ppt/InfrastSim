namespace InfrastSim.TimeDriven.Operators;
internal class Rosmontis : OperatorBase {
    public override string Name => "迷迭香";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            var sum = simu.Dormitories.Sum(fac => fac?.Operators.Count() ?? 0);
            simu.Ganzhixinxi.SetValue(Name, sum);

            simu.Delay(simu => {
                simu.Siweilianhuan.SetValue(Name, simu.Ganzhixinxi);
            }, Priority.PropConversion);

            simu.Delay(simu => {
                EfficiencyModifier.SetValue(Name, simu.Siweilianhuan / (Upgraded >= 2 ? 1 : 2));
            });
        }
    }
}
