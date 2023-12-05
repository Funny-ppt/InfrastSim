namespace InfrastSim.TimeDriven.Operators;
internal class Coldshot : OperatorBase {
    public override string Name => "冰酿";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ControlCenter center && !IsTired) {
            center.ExtraMoodModifier.SetValue(Name, -0.05);
        }
        if (Facility is Dormitory dorm && Upgraded >= 2) {
            // FIXME: 冰酿自己是否为反嘲讽？
            var count = dorm.Operators.Where(op => !op.IsFullOfEnergy).Count();
            if (count == 1) {
                dorm.SetVipMoodModifier(-0.8);
            } else if (count > 1) {
                var amount = -0.8 / count;
                dorm.SetDormMoodModifier(amount);
            }
        }
    }
}
