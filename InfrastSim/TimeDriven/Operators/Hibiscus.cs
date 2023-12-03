namespace InfrastSim.TimeDriven.Operators;
internal class Hibiscus : OperatorBase {
    public override string Name => "芙蓉";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-0.55);
        }

        // TODO: missing skill 2
    }
}

