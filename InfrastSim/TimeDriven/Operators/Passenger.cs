namespace InfrastSim.TimeDriven.Operators;
internal class Passenger : OperatorBase {
    public override string Name => "异客";
    static string[] _groups = { "依赖设施数量" }; // TODO
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);


        if (Facility is PowerStation && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 0.15 : 0.1);
        }

        if (Facility is ManufacturingStation manufacturing && !IsTired && Upgraded >= 2) {
            simu.Delay(simu => {
                foreach (var op in manufacturing.Operators) {
                    if (!op.Groups.Contains("依赖设施数量")) {
                        op.EfficiencyModifier.MaxValue = 0;
                    }
                }
                EfficiencyModifier.SetValue(Name, 0.05 * simu.GetPowerStations());
            }, Priority.AccordingToFacilityAmount);
        }
    }
}
