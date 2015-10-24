using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace HmxSynchWPF
{
    public class HmxWindowManager : WindowManager, IHmxWindowManager
    {
        private Window MainWindow { get; set; }
        protected override Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
        {
            var window = base.CreateWindow(rootModel, isDialog, context, settings);
            ResourceDictionary resourceDictionary = App.Current.Resources.MergedDictionaries.First();
            window.Style = resourceDictionary["WindowStyle"] as Style;
            if (MainWindow == null)
            {
                MainWindow = window;
            }
            return window;
        }

        public void MakeTrayApp()
        {
            if (MainWindow != null)
            {
                MainWindow.Hide();
            }
        }
    }
}