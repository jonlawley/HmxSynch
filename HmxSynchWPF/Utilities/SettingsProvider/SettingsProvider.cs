using System.Configuration;

namespace HmxSynchWPF.Utilities.SettingsProvider
{
    public class SettingsProvider : ISettingsProvider
    {
        public string GetSetting(string key)
        {
            var ass = Properties.Settings.Default[key];

            return ass.ToString();
        }

        public void SetSetting(string key, string value)
        {
            Properties.Settings.Default[key] = value;
            Properties.Settings.Default.Save();
        }
    }
}