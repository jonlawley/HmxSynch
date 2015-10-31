using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HmxSynchWPF.Annotations;
using HmxSynchWPF.Data;
using HmxSynchWPF.Models;
using HmxSynchWPF.Utilities.SettingsProvider;
using HmxSynchWPF.Utilities.Task;
using VLCDriver;

namespace HmxSynchWPF.RecordingManager
{
    public class RecordingManager : IRecordingManager, INotifyPropertyChanged
    {
        private readonly log4net.ILog _log;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IVlcDriver _vlcDriver;
        private readonly ILinearJobManager _linearJobManager;
        private readonly ITaskRunner _taskRunner;
        private readonly HumaxDirSource _hmDirSource;
        private bool _synchInProgress;

        public RecordingManager(log4net.ILog log, ISettingsProvider settingsProvider, IVlcDriver vlcDriver, ILinearJobManager linearJobManager, ITaskRunner taskRunner)
        {
            _log = log;
            _settingsProvider = settingsProvider;
            _vlcDriver = vlcDriver;
            _linearJobManager = linearJobManager;
            _taskRunner = taskRunner;
            _hmDirSource = new HumaxDirSource
            {
                Location = _settingsProvider.GetSetting("HumaxDirSource")
            };
        }

        public void Synch()
        {
            if (!SynchInProgress)
            {
                _taskRunner.Start(new Task(SynchInternal), _log);
                _log.Debug("Synch Not already in progress, starting new");
                SynchInProgress = true;
            }
            else
            {
                _log.Debug("Not going to synch as there's one in progress already");
            }
        }

        private void SynchInternal()
        {
            var ready = _hmDirSource.SourceReady();
            if (!ready)
            {
                _log.Debug("Humax Source not ready. Cancelling synch");
                _log.Info("Could not conect to Humax, is it turned on and connected?");
                SynchInProgress = false;
                return;
            }

            _log.Debug("Humax source ready, downloading episodes currently on box");
            _hmDirSource.UpdateEpisodeList();
            _log.Debug(string.Format("Updated, there are {0} episodes", _hmDirSource.Episodes));
            //TODO should get all programmes
            var hmxEpisodes = _hmDirSource.Episodes;

            List<Episode> dbEpisodes;
            using (var db = new HmxContext())
            {
                dbEpisodes = db.Episodes.ToList();
                foreach (var episode in hmxEpisodes)
                {
                    if (dbEpisodes.All(x => x.FilePath != episode.FilePath))
                    {
                        db.Episodes.Add(episode);
                    }
                }
                foreach (var episode in dbEpisodes)
                {
                    if (hmxEpisodes.All(x => x.FilePath != episode.FilePath))
                    {
                        db.Episodes.Remove(episode);
                    }
                }
                db.SaveChanges();
                dbEpisodes = db.Episodes.ToList();
            }

            var episodesToConvert = dbEpisodes.Where(x => x.Convert).ToList();

            foreach (var episode in episodesToConvert)
            {
                episode.GenerateFinalOutputPath(_settingsProvider.GetSetting("MainOutputDir"));
                if (File.Exists(episode.FinalOutputPath))
                {
                    var info = new FileInfo(episode.FinalOutputPath);
                    const long threshholdSmallestValidMediaFile = 500000;
                    if (_vlcDriver.JobBag.Any(x => x.OutputFile.FullName == info.FullName))
                    {
                        episode.Convert = false;
                        continue;
                    }

                    if (info.Length < threshholdSmallestValidMediaFile)
                    {
                        info.Delete();
                    }
                    else
                    {
                        episode.Convert = false;
                    }
                }
            }

            episodesToConvert = episodesToConvert.Where(x => x.Convert).ToList();

            foreach (var episode in episodesToConvert)
            {
                _hmDirSource.CopyEpisodeToTempDir(episode);
            }

            foreach (var episode in episodesToConvert)
            {
                VlcJob job;
                if (episode.ConvertAudio)
                {
                    var vlcAudioJob = _vlcDriver.CreateAudioJob();
                    vlcAudioJob.AudioConfiguration = new AudioConfiguration
                    {
                        AudioBitrateInkbps = 192,
                        Format = AudioConfiguration.ConversionFormats.Mp3
                    };
                    job = vlcAudioJob;
                }
                else
                {
                    var vlcVideoJob = _vlcDriver.CreateVideoJob();
                    vlcVideoJob.AudioConfiguration = new AudioConfiguration
                    {
                        AudioBitrateInkbps = 92
                    };
                    vlcVideoJob.VideoConfiguration = new VideoConfiguration
                    {
                        Format = VideoConfiguration.VlcVideoFormat.h264,
                        Scale = VideoConfiguration.VideoScale.quarter
                    };

                    job = vlcVideoJob;
                }
                job.InputFile = new FileInfo(episode.IntermediateTempPath);
                job.OutputFile = new FileInfo(episode.FinalOutputPath);
                _linearJobManager.Start();
            }
            SynchInProgress = false;
        }

        public bool SynchInProgress
        {
            get
            {
                return _synchInProgress;
            }
            private set
            {
                _synchInProgress = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}