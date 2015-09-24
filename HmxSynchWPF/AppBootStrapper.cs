using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Windows;
using Caliburn.Micro;

namespace HmxSynchWPF
{
    class AppBootStrapper : BootstrapperBase
    {
        private IWindowManager _windowManager;
        private readonly SimpleContainer _container = new SimpleContainer();

        public AppBootStrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            base.Configure();

            _windowManager = new HmxWindowManager();
            _container.RegisterInstance(typeof(IWindowManager), null, _windowManager);
            _container.RegisterInstance(typeof(IContainer), null, _container);
            _container.RegisterInstance(typeof(IAppViewModel), null, new AppViewModel(_windowManager));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var settings = new Dictionary<string, object>
           {
               { "SizeToContent", SizeToContent.Manual },
               { "Height" , 600  },
               { "Width"  , 400 },
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
    }
}
