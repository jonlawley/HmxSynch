using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace HmxSynchWPF
{
    public class HmxWindowManager : WindowManager
    {
        protected override Window CreateWindow(object rootModel, bool isDialog, object context, IDictionary<string, object> settings)
        {
            var window = base.CreateWindow(rootModel, isDialog, context, settings);
            ResourceDictionary resourceDictionary = App.Current.Resources.MergedDictionaries.First();
            window.Style = resourceDictionary["WindowStyle"] as Style;
            return window;
        }
    }
}