namespace InfrastSim.TimeDriven.Operators;
internal class Morgan : OperatorBase {
    public override string Name => "摩根";
    static string[] _groups = { "格拉斯哥帮" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            var siege = dorm.FindOp("推进之王");
            if (siege != null) {
                var amount = siege.Upgraded >= 2 ? -20 : -15;
                simu.Delay(simu => {
                    foreach (var op in dorm.GroupMembers("格拉斯哥帮")) {
                        op.MoodConsumeRate.SetIfLesser("dorm-extra", amount + -30); // FIXME? 和冰酿的交互
                    }
                });
            }
        }
        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            var amount = trading.GroupMemberCount("格拉斯哥帮") * 20
                + (trading.FindOp("推进之王") != null ? 35 : 0);
            EfficiencyModifier.SetValue(Name, amount);
        }
    }
}
