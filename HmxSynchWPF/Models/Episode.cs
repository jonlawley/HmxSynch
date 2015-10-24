using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace HmxSynchWPF.Models
{
    public class Episode 
    {
        [Key]
        public int Id {get;set;}

        public string Name { get; set; }
        public DateTime Time { get; set; }
        public bool Converted { get; set; }
        public bool Convert { get; set; }
        public bool ConvertAudio { get; set; }

        public string FilePath { get; set; }

        [NotMapped]
        public string FinalOutputPath { get; set; }

        [NotMapped]
        public string IntermediateTempPath { get; set; }

        public void GenerateFinalOutputPath(string outputDir)
        {
            if (!Directory.Exists(outputDir))
            {
                throw new DirectoryNotFoundException("Need to provide input directory");
            }

            var ext = ConvertAudio ? ".mp3" : ".mp4";

            var path = Time.ToString("yyyyMMddT HH:mm") + " " + Name + ext;

            FinalOutputPath = Path.Combine(outputDir, path);
        }
    }
}