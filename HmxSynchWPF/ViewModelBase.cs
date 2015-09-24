using Caliburn.Micro;

namespace HmxSynchWPF
{
    public class ViewModelBase
    {
        public ViewModelBase(IWindowManager windowManager)
        {
            WindowManager = windowManager;
        }
        protected IWindowManager WindowManager {  get; private set; }
    }
}