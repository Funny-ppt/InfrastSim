namespace InfrastSim.TimeDriven.Operators;

internal class Minimalist : OperatorBase {
    public override string Name => "至简";
    static string[] _groups = { "杜林" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            var amount = simu.Facilities.Sum(fac => fac?.Level ?? 0);
            simu.Gongchengjiqiren.SetValue(Name, Math.Min(64, amount));

            EfficiencyModifier.SetValue(Name, Util.Align(simu.Gongchengjiqiren, Upgraded >= 2 ? 8 : 16));
        }
    }
}
