using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastSim.TimeDriven {
    internal record struct CompositionSkillInfo(int UnlockUpgraded, SkillInfo Skill) {
        public static implicit operator CompositionSkillInfo(SkillInfo skill) => new(0, skill);
        public static implicit operator CompositionSkillInfo((int UnlockUpgraded, SkillInfo Skill) tuple) => new(tuple.UnlockUpgraded, tuple.Skill);
    }
}
