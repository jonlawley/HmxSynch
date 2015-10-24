using System.Collections.Generic;

namespace HmxSynchWPF.Models
{
    class Series
    {
        public string Name { get; set; }

        public IList<Episode> Episodes { get; set; }
    }
}
