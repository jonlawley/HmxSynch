using System;
using System.Threading.Tasks;
using log4net;

namespace HmxSynchWPF.Utilities.Task
{
    public class TaskRunner : ITaskRunner
    {
        private ILog _log;

        public void Start(System.Threading.Tasks.Task task, ILog log)
        {
            _log = log;
            task.ContinueWith(t => InvokeError(t, t.Exception.InnerException),
                           TaskContinuationOptions.OnlyOnFaulted |
                           TaskContinuationOptions.ExecuteSynchronously);
            task.Start();
        }

        private void InvokeError(System.Threading.Tasks.Task task, Exception innerException)
        {
            _log.Error("Error occurred when synching", innerException);
        }
    }
}