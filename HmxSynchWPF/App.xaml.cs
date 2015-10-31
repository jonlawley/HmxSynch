using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;

namespace HmxSynchWPF
{
    public partial class App
    {
        public App()
        {
           // var tb = (TaskbarIcon)FindResource("MyNotifyIcon");
        }
        ICommand Show { get; set; }
    }
}
