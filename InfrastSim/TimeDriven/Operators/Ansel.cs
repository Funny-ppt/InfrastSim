namespace InfrastSim.TimeDriven.Operators;
internal class Ansel : OperatorBase {
    public override string Name => "安塞尔";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-55);
        }

        // TODO: missing skill 2
    }
}

