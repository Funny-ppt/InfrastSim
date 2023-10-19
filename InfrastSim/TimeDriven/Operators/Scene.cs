namespace InfrastSim.TimeDriven.Operators;

internal class Scene : OperatorBase {
    public override string Name => "稀音";
    public static string Annouce => "稀音小姐非常可爱哦";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            var time = Math.Min(5, Math.Floor(WorkingTime / TimeSpan.FromHours(1)));
            EfficiencyModifier.SetValue(Name, 0.15 + time * 0.02);

            if (Upgraded >= 2) {
                if (manufacturing.Product?.Name.Contains("记录") ?? false) {
                    manufacturing.Capacity.SetValue(Name, 12);
                }
            }
        }
    }
}