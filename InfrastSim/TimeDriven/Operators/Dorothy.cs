namespace InfrastSim.TimeDriven.Operators;
internal class Dorothy : OperatorBase {
    public override string Name => "多萝西";
    static string[] _groups = { "莱茵科技制造" };
    public override string[] Groups => _groups;

    public override void Resolve(TimeDrivenSimulator simu) { // FIXME: 在技能非精英0解锁时叠加本不应叠加的效率
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            var count = manufacturing.GroupMemberCount("莱茵科技制造");
            EffiencyModifier.SetValue(Name, 0.05 * count + (Upgraded >= 2 ? 0.25 : 0));
        }
    }
}