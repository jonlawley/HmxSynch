using Caliburn.Micro;

namespace HmxSynchWPF
{
    public class ViewModelBase : Screen
    {
        public ViewModelBase(IHmxWindowManager windowManager)
        {
            WindowManager = windowManager;
        }
        protected IHmxWindowManager WindowManager { get; private set; }
    }
}