namespace InfrastSim.TimeDriven.Operators;
internal class Hoederer : OperatorBase {
    public override string Name => "赫德雷";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.25);
            if (simu.OperatorsInFacility.Any(op => op.Name == "伊内丝")) {
                EfficiencyModifier.AddValue(Name, 0.05);
            }

            if (Upgraded >= 2) {
                EfficiencyModifier.AddValue(Name, 0.05);
                if (simu.OperatorsInFacility.Any(op => op.Name == "W")) {
                    EfficiencyModifier.AddValue(Name, 0.05);
                }
            }
        }
    }
}
