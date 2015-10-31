namespace HmxSynchWPF.Utilities.SettingsProvider
{
    public interface ISettingsProvider
    {
        string GetSetting(string key);
        void SetSetting(string key, string value);
    }
}