using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastSim.TimeDriven {
    internal interface ITimeDrivenObject {
        void Update(TimeElapsedInfo info);
    }
}
