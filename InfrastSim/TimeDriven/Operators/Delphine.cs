using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastSim.TimeDriven.Operators {
    internal class Delphine : OperatorBase {
        public override string Name => "戴菲恩";

        public override void Resolve(TimeDrivenSimulator simu) {
            base.Resolve(simu);

            if (Facility?.Type == FacilityType.ControlCenter && !IsTired && Upgraded >= 2) {
                var ops = simu.Operators.Where(op => op.Facility is TradingStation && op.Groups.Contains("格拉斯哥帮"));
                foreach (var op in ops) {
                    op.EffiencyModifier.SetValue(Name, 0.1);
                }
            }
        }
    }
}
