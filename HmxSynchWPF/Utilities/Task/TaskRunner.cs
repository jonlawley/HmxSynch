using System;
using System.Threading.Tasks;
using NLog;

namespace HmxSynchWPF.Utilities.Task
{
    public class TaskRunner : ITaskRunner
    {
        private ILogger _log;

        public void Start(System.Threading.Tasks.Task task, ILogger log)
        {
            _log = log;
            task.ContinueWith(t => InvokeError(t, t.Exception.InnerException),
                           TaskContinuationOptions.OnlyOnFaulted |
                           TaskContinuationOptions.ExecuteSynchronously);
            task.Start();
        }

        private void InvokeError(System.Threading.Tasks.Task task, Exception innerException)
        {
            _log.Error(innerException, "Error occurred when synching");
        }
    }
}