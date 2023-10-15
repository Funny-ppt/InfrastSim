namespace InfrastSim.TimeDriven.Operators;
internal class Purestream : OperatorBase {
    public override string Name => "清流";
    static string[] _groups = { "依赖设施数量" };
    public override string[] Groups => _groups;

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility is PowerStation) {
            EffiencyModifier.SetValue(Name, 0.15);
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 1) {
            if (manufacturing.Product == Product.Gold) {
                EffiencyModifier.SetValue(Name, simu.GetPowerStations() * 0.2);
            }
        }
    }
}
