using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HmxSynchWPF.Data;
using HmxSynchWPF.RecordingManager;
using HmxSynchWPF.Utilities.SettingsProvider;
using HmxSynchWPF.Utilities.Timer;
using ILog = log4net.ILog;

namespace HmxSynchWPF
{
    public class AppViewModel : ViewModelBase, IAppViewModel
    {
        private bool _isClosing;
        public IRecordingManager RecordingManager { get; private set; }
        private readonly ILog _logger;
        private ITimer _pollingTimer;
        private readonly ISettingsProvider _settingsProvider;

        private bool _trayIconVisible;
        public bool TrayIconVisible
        {
            get
            {
                return _trayIconVisible;
            }
            set
            {
                _trayIconVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public AppViewModel(IHmxWindowManager windowManager, IRecordingManager recordingManager, ILog logger, ITimer timer, ISettingsProvider settingsProvider)
            : base(windowManager)
        {
            RecordingManager = recordingManager;
            _settingsProvider = settingsProvider;

            _logger = logger;
            _pollingTimer = timer;
            _pollingTimer.TimerElapsed += PollingTimerElapsed;
            var setting = settingsProvider.GetSetting("PollingTimeSpanInMinutes");
            _pollingTimer.Interval = TimeSpan.FromMinutes(Convert.ToDouble(setting)).TotalMilliseconds;

            new SQLiteConfiguration();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && (args[1] == "/q" || args[1] == "-q"))
            {
                MakeTrayApp();
            }
        }

        private void MakeTrayApp()
        {
            WindowManager.MakeTrayApp();
            TrayIconVisible = true;
        }

        private void PollingTimerElapsed(object sender, EventArgs e)
        {
            _logger.Debug("Poller Elapsed");
            if (!RecordingManager.SynchInProgress)
            {
                RecordingManager.Synch();
            }
            else
            {
                _logger.Debug("Not synching because there is already one in progress");
            }
        }

        public void ShowApp()
        {
            TrayIconVisible = false;
            WindowManager.RestoreFromTray();
        }

        public override void CanClose(Action<bool> callback)
        {
            if (!_isClosing)
            {
                MakeTrayApp();
            }
            else
            {
                base.CanClose(callback);
            }
        }

        public void Close()
        {
            _pollingTimer.Stop();
            _pollingTimer.Close();
            _pollingTimer = null;
            _isClosing = true;
            base.TryClose();
        }

        private ICollection<Tuple<string, int>> _pollingInvervalOptions;
        public ICollection<Tuple<string, int>> PollingInvervalOptions
        {
            get
            {
                return _pollingInvervalOptions ?? (_pollingInvervalOptions = new Collection<Tuple<string, int>>
                {
                    new Tuple<string, int>("Disabled", -1),
                    new Tuple<string, int>("1 Minute", 1),
                    new Tuple<string, int>("5 Minutes", 5),
                    new Tuple<string, int>("15 Minutes", 15),
                    new Tuple<string, int>("30 Minutes", 30),
                    new Tuple<string, int>("1 Hour", 60)
                });
            }
        }

        public Tuple<string, int> SelectedPollingIntervalItem
        {
            get
            {
                var settingsPollingInterval = Convert.ToInt32(_settingsProvider.GetSetting("PollingTimeSpanInMinutes"));
                var selectedPollingIntervalItem = PollingInvervalOptions.FirstOrDefault(x => x.Item2 == settingsPollingInterval);
                if (selectedPollingIntervalItem == null)
                {
                    selectedPollingIntervalItem = PollingInvervalOptions.First();
                }
                return selectedPollingIntervalItem;
            }
            set
            {
                _settingsProvider.SetSetting("PollingTimeSpanInMinutes", value.Item2.ToString());
            }
        }
    }
}