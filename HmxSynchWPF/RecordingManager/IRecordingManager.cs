namespace HmxSynchWPF.RecordingManager
{
    public interface IRecordingManager
    {
        void Synch();
        bool SynchInProgress { get; }
    }
}