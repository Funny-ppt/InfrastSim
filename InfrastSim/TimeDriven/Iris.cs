namespace InfrastSim.TimeDriven.Operators;
internal class Iris : OperatorBase {
    public override string Name => "爱丽丝";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetDormMoodModifier(-0.1);
            simu.Mengjing.AddValue(Name, dorm.Level);
            if (Upgraded >= 2) {
                simu.Ganzhixinxi.AddValue(Name, simu.Mengjing);
            }
        }
    }
}
