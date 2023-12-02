namespace InfrastSim.TimeDriven.Operators;
internal class Lumen : OperatorBase {
    public override string Name => "流明";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            simu.Delay(simu => {
                foreach (var op in dorm.Operators) {
                    var amount = simu.PowerStationsCount() * -0.05 + (Upgraded >= 2 ? -0.15 : -0.1);
                    op.MoodConsumeRate.SetIfLesser("dorm-extra", amount);
                }
            });
        }
    }
}

