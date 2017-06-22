using System;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public class BlueprintTemplate
    {
        public StaticTypeData Type { get; }
        public TimeSpan? CopyTime { get; }
        public int MaxProductionLimit { get; }

        public BlueprintTemplate(StaticTypeData type, TimeSpan? copyTime, int maxProductionLimit)
        {
            Type = type;
            CopyTime = copyTime;
            MaxProductionLimit = maxProductionLimit;
        }
    }
}