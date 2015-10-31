using System;
using System.Timers;

namespace HmxSynchWPF.Utilities.Timer
{
    public class Timer : ITimer
    {
        private System.Timers.Timer _timer;

        public Timer()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += pollingTimer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        private void pollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TimerElapsed != null)
            {
                TimerElapsed(sender, e);
            }
        }

        public event EventHandler TimerElapsed;
        public void Close()
        {
            _timer.Close();
            _timer = null;
        }
    }
}