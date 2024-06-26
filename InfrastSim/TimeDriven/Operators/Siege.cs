namespace InfrastSim.TimeDriven.Operators;
internal class Siege : OperatorBase {
    public override string Name => "推进之王";
    static string[] _groups = { "格拉斯哥帮" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetDormMoodModifier(Upgraded >= 2 ? -20 : -15);
        }
    }
}
