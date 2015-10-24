using System;

namespace HmxSynchWPF
{
    static class UtilityMethods
    {
        public static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            
            return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString());
        }
    }
}
