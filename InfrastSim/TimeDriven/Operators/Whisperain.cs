namespace InfrastSim.TimeDriven.Operators;

internal class Whisperain : OperatorBase {
    public override string Name => "絮雨";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Office office && !IsTired) {
            EfficiencyModifier.SetValue(Name, 20);
            simu.Jiyisuipian.SetValue(Name, 20); // FIXME: hardcoded

            if (Upgraded >= 2) {
                simu.Ganzhixinxi.AddValue(Name, simu.Jiyisuipian);
            }
        }
    }
}