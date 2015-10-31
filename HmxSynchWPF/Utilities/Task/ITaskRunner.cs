namespace HmxSynchWPF.Utilities.Task
{
    public interface ITaskRunner
    {
        void Start(System.Threading.Tasks.Task task, log4net.ILog log);
    }
}