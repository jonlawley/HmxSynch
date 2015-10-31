using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using HmxSynchWPF.Models;
using HmxSynchWPF.Utilities;

namespace HmxSynchWPF
{
    class HumaxDirSource
    {
        public HumaxDirSource()
        {
            Episodes = new List<Episode>();
            Series = new List<Series>();
        }

        public string Location { get; set; }
        public IList<Episode> Episodes { get;private set; }
        public IList<Series> Series { get; private set; }

        public void UpdateEpisodeList()
        {
            if (Location == null || !Directory.Exists(Location))
            {
                throw new InvalidOperationException("Need to set 'Location'");
            }
            var humaxDir = new DirectoryInfo(Location);
            var filesOnHumax = humaxDir.GetFiles("*.ts", SearchOption.TopDirectoryOnly);
            Episodes.Clear();
            foreach (var fileInfo in filesOnHumax)
            {
                Episodes.Add(EpisodeFactory(fileInfo));
            }
            var subDirectories = humaxDir.GetDirectories("*", SearchOption.AllDirectories).Where(x => !x.FullName.Contains("[Deleted Items]")); ;
            foreach (var directory in subDirectories)
            {
                var directoryFiles = directory.GetFiles();
                if (directoryFiles.Any())
                {
                    var series = SeriesFactory(directory);
                    Series.Add(series);
                    foreach (var fileInfo in directoryFiles)
                    {
                        series.Episodes.Add(EpisodeFactory(fileInfo));
                    }
                }
            }
        }

        private Episode EpisodeFactory(FileInfo file)
        {
            var episode = new Episode();
            var originalFileName = Path.GetFileNameWithoutExtension(file.Name);
            var sections = originalFileName.Split('_');
            episode.FilePath = UtilityMethods.MakeRelative(file.FullName, Location);
            bool canParse = false;
            if (sections.Length == 3)
            {
                episode.Name = sections[0];
                var dt = string.Format("{0}{1}", sections[1], sections[2]);
                DateTime time;
                
                canParse = DateTime.TryParseExact(dt,
                    "yyyyMMddHHmm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out time);
                episode.Time = time;
            }
            if (!canParse)
            {
                episode.Name = originalFileName;
            }
            return episode;
        }

        private Series SeriesFactory(DirectoryInfo dirInfo)
        {
            return new Series {Name = dirInfo.Name, Episodes = new List<Episode>()};
        }

        public void CopyEpisodeToTempDir(Episode episode)
        {
            string destFileName = ConfigurationManager.AppSettings["ScratchDir"];
            File.Copy(episode.FilePath, destFileName);
            episode.IntermediateTempPath = Path.Combine(destFileName, episode.Name);
        }

        public bool SourceReady()
        {
            return Location!=null && Directory.Exists(Location);
        }
    }
}
