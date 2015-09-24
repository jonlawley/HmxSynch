using Caliburn.Micro;

namespace HmxSynchWPF
{
    public class AppViewModel : ViewModelBase, IAppViewModel
    {
        public AppViewModel(IWindowManager windowManager)
            : base(windowManager)
        {           
        }

        public void SynchNow()
        {
            
        }
    }
}