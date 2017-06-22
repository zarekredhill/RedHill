using System;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public class BlueprintTemplate
    {
        public StaticTypeData Type { get; }
        public int MaxProductionLimit { get; }
        public TimeSpan? CopyTime { get; }
        public TimeSpan? MaterialResearchTime {get;}
        public TimeSpan? TimeResearchTime {get;}

        public BlueprintTemplate(StaticTypeData type, int maxProductionLimit, TimeSpan? copyTime, TimeSpan? materialResearchTime, TimeSpan? timeResearchTime)
        {
            Type = type;
            MaxProductionLimit = maxProductionLimit;
            CopyTime = copyTime;
            MaterialResearchTime = materialResearchTime;
            TimeResearchTime = timeResearchTime;            
        }
    }
}
