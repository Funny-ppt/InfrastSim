namespace InfrastSim.TimeDriven.Operators;
internal class Czerny : OperatorBase {
    public override string Name => "车尔尼";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-0.65);
            simu.Xiaojie.AddValue(Name, dorm.Level);
            if (Upgraded >= 2) {
                simu.Ganzhixinxi.AddValue(Name, simu.Xiaojie);
            }
        }
    }
}
