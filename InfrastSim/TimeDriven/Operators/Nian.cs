namespace InfrastSim.TimeDriven.Operators;
internal class Nian : OperatorBase {
    public override string Name => "年";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1 & 2
    }
}