using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Timers;
using Caliburn.Micro;
using HmxSynchWPF.Data;
using HmxSynchWPF.Models;
using VLCDriver;
using ILog = log4net.ILog;

namespace HmxSynchWPF
{
    public class AppViewModel : ViewModelBase, IAppViewModel
    {
        private DateTime? lastSynchTime;
        private Timer pollingTimer;
        private volatile bool synchInProgress;
        private HumaxDirSource hmDirSource;
        private VlcDriver vlCDriver;
        private LinearJobManager jobManager;
        private ILog logger;

        public AppViewModel(IHmxWindowManager windowManager)
            : base(windowManager)
        {
            logger = log4net.LogManager.GetLogger(typeof(log4net.Appender.FileAppender));
            hmDirSource = new HumaxDirSource
            {
                Location = @"C:\secure\Source\Offline\Humax2Gdrive\Humax2Gdrive\Humax2GdriveTests\HumaxtestDir\My Video"
            };

            pollingTimer = new Timer();
            pollingTimer.Interval = new TimeSpan(0,1,0).TotalMilliseconds;
            pollingTimer.Elapsed += pollingTimer_Elapsed;
            pollingTimer.Start();

            vlCDriver = new VlcDriver();

            jobManager = new LinearJobManager(vlCDriver);

            new SQLiteConfiguration();
        }

        private void pollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SynchNow()
        {
            logger.Debug("SynchNow() Call");
            Synch();
            logger.Debug("Completed SynchNow() Call");
        }

        private void Synch()
        {
            if (!synchInProgress)
            {
                logger.Debug("Synch Not already in progress, starting new");
                synchInProgress = true;

                var ready = hmDirSource.SourceReady();
                if (!ready)
                {
                    logger.Debug("Humax Source not ready. Cancelling synch");
                    logger.Info("Could not conect to Humax, is it turned on and connected?");
                    synchInProgress = false;
                    return;
                }

                logger.Debug("Humax source ready, downloading episodes currently on box");
                hmDirSource.UpdateEpisodeList();
                logger.Debug(string.Format("Updated, there are {0} episodes", hmDirSource.Episodes)); //TODO should get all programmes
                var hmxEpisodes = hmDirSource.Episodes;
                
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
                    episode.GenerateFinalOutputPath(ConfigurationManager.AppSettings["MainOutputDir"]);
                    if (File.Exists(episode.FinalOutputPath))
                    {
                        var info = new FileInfo(episode.FinalOutputPath);
                        const long threshholdSmallestValidMediaFile = 500000;
                        if (vlCDriver.JobBag.Any(x => x.OutputFile.FullName == info.FullName))
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
                    hmDirSource.CopyEpisodeToTempDir(episode);
                }

                foreach (var episode in episodesToConvert)
                {
                    VlcJob job;
                    if (episode.ConvertAudio)
                    {
                        var vlcAudioJob = vlCDriver.CreateAudioJob();
                        vlcAudioJob.AudioConfiguration = new AudioConfiguration
                        {
                            AudioBitrateInkbps = 192,
                            Format = AudioConfiguration.ConversionFormats.Mp3
                        };
                        job = vlcAudioJob;
                    }
                    else
                    {
                        var vlcVideoJob = vlCDriver.CreateVideoJob();
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
                    jobManager.Start();

                }
                synchInProgress = false;
            }
            else
            {
                logger.Debug("Not going to synch as there's one in progress already");
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            //WindowManager.MakeTrayApp();
           base.CanClose(callback);
        }

        public void Close()
        {
            base.TryClose();
        }
    }
}