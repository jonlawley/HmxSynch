using System.Linq;
using VLCDriver;

namespace HmxSynchWPF
{
    public class LinearJobManager
    {
        private VlcDriver driver;
        public LinearJobManager(VlcDriver vlcdriver)
        {
            driver = vlcdriver;
            vlcdriver.OnJobStateChange += driver_OnJobStateChange;
        }

        private void driver_OnJobStateChange(object source, JobStatusChangedEventArgs e)
        {
            if (driver.JobBag.Contains(e.Job))
            {
                if (e.Job.State == VlcJob.JobState.Finished)
                {
                    Start();
                }
            }
        }

        public void Start()
        {
            if (driver.JobBag.All(x => x.State != VlcJob.JobState.Started))
            {
                var jobToStart = driver.JobBag.FirstOrDefault(x => x.State == VlcJob.JobState.NotStarted);
                if (jobToStart != null)
                {
                    driver.StartJob(jobToStart);
                }
            }
        }       
    }
}