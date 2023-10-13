using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastSim.TimeDriven; 
internal abstract class OperatorBase : ITimeDrivenObject {
    public string Name { get; }
    public FacilityBase? Facility { get; private set; } = null;

    const double MinMood = 0.0;
    const double MaxMood = 24.0;
    public double Mood { get; private set; } = 24.0;
    public bool IsTired => Util.Equals(0.0, Mood);
    public AggregateValue MoodConsumeRate { get; } = new(1.0);
    public AggregateValue ManufacturingFactor { get; } = new(1.0);
    public AggregateValue TradingFactor { get; } = new(1.0);
    public AggregateValue RecuritFactor { get; } = new(1.0);
    public AggregateValue IntelligenceFactor { get; } = new(1.0);

    public void Update(TimeElapsedInfo info) {
        if (Facility != null && Facility.IsWorking) {
            var newMood = Mood + MoodConsumeRate * (info.TimeElapsed / TimeSpan.FromHours(1));
            Mood = Math.Clamp(newMood, MinMood, MaxMood);
        }
    }

}