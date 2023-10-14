namespace InfrastSim.TimeDriven;
internal interface ITimeDrivenObject {
    void Resolve(TimeDrivenSimulator simu);
    void Update(TimeDrivenSimulator simu, TimeElapsedInfo info);
}
