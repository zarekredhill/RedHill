using System;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public class BlueprintTemplate
    {
        public StaticTypeData Type { get; }
        public TimeSpan? CopyTime { get; }

        public BlueprintTemplate(StaticTypeData type, TimeSpan? copyTime)
        {
            Type = type;
            CopyTime = copyTime;
        }
    }
}