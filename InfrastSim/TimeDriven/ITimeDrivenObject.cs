namespace InfrastSim.TimeDriven;
internal interface ITimeDrivenObject {
    void Reset();
    void Resolve(TimeDrivenSimulator simu);
    void Update(TimeDrivenSimulator simu, TimeElapsedInfo info);
}
