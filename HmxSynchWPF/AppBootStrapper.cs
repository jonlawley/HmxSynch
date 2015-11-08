using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Caliburn.Micro;
using HmxSynchWPF.RecordingManager;
using HmxSynchWPF.Utilities.SettingsProvider;
using HmxSynchWPF.Utilities.Task;
using HmxSynchWPF.Utilities.Timer;
using VLCDriver;
using LogManager = NLog.LogManager;

namespace HmxSynchWPF
{
    class AppBootStrapper : BootstrapperBase
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private IHmxWindowManager _windowManager;
        private readonly SimpleContainer _container = new SimpleContainer();

        public AppBootStrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            base.Configure();

            _windowManager = new HmxWindowManager();
            var logger = LogManager.GetCurrentClassLogger();
            _container.RegisterPerRequest(typeof(ITaskRunner), null, typeof(TaskRunner));
            _container.RegisterInstance(typeof(IWindowManager), null, _windowManager);
            _container.RegisterInstance(typeof(IHmxWindowManager), null, _windowManager);
            _container.RegisterInstance(typeof(ILog), null, logger);
            _container.RegisterPerRequest(typeof(ISettingsProvider), null, typeof(SettingsProvider));
            _container.RegisterPerRequest(typeof(ITimer), null, typeof(Timer));
            _container.RegisterPerRequest(typeof(IVlcDriver), null, typeof(VlcDriver)); // TODO Should be one per app
            _container.RegisterPerRequest(typeof(ILinearJobManager), null, typeof(LinearJobManager));
            _container.RegisterPerRequest(typeof(IRecordingManager), null, typeof(RecordingManager.RecordingManager));
            _container.RegisterPerRequest(typeof(IAppViewModel),null, typeof(AppViewModel));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var priorProcess = PriorProcess();
            if (priorProcess != null)
            {
                //TODO Show Main Window if hidden in taskbar
                SetForegroundWindow(priorProcess.MainWindowHandle);
                Application.Shutdown();
                return;
            }

            var settings = new Dictionary<string, object>
           {
               { "SizeToContent", SizeToContent.Manual },
               { "Height" , 600  },
               { "Width"  , 400 },
               { "Title", Properties.Resources.MainWindowTitle_HumaxSynch},
               { "Owner", null }
           };

            DisplayRootViewFor<IAppViewModel>(settings);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        public static Process PriorProcess()
        {
            var curr = Process.GetCurrentProcess();
            var procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }
    }
}
