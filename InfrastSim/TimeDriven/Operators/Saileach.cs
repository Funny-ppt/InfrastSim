namespace InfrastSim.TimeDriven.Operators;
internal class Saileach : OperatorBase {
    public Saileach() { }
    public override string Name => "琴柳";
    public override int DormVipPriority => 0;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-70);
        }
    }
}
