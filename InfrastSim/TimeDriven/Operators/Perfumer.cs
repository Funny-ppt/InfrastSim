namespace InfrastSim.TimeDriven.Operators;
internal class Perfumer : OperatorBase {
    public override string Name => "调香师";
    static string[] _groups1 = { "标准化" };
    public override string[] Groups => Upgraded >= 1 ? _groups1 : base.Groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            EfficiencyModifier.SetValue(Name, 25);
        }
    }
}

