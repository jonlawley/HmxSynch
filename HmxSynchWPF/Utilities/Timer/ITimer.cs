using System;

namespace HmxSynchWPF.Utilities.Timer
{
    public interface ITimer
    {
        void Start();
        void Stop();
        double Interval { get; set; }
        event EventHandler TimerElapsed;
        void Close();
    }
}