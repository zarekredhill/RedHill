using System;
using System.Collections.Immutable;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public class BlueprintTemplate
    {
        public TypeInfo Type { get; }
        public int MaxProductionLimit { get; }
        public TimeSpan? CopyTime { get; }
        public TimeSpan? MaterialResearchTime { get; }
        public TimeSpan? TimeResearchTime { get; }
        public ManufacturingDetails Manufacturing { get; }

        public BlueprintTemplate(TypeInfo type, int maxProductionLimit, TimeSpan? copyTime, TimeSpan? materialResearchTime, TimeSpan? timeResearchTime, ManufacturingDetails manufacturing)
        {
            Type = type;
            MaxProductionLimit = maxProductionLimit;
            CopyTime = copyTime;
            MaterialResearchTime = materialResearchTime;
            TimeResearchTime = timeResearchTime;
            Manufacturing = manufacturing;
        }

        public class ManufacturingDetails
        {
            public TimeSpan Time { get; }
            public TypeInfo ProductType { get; }
            public int ProductQuantity { get; }
            public ImmutableList<Skill.Requirement> Requirements { get; }

            public ManufacturingDetails(TimeSpan time, TypeInfo productType, int productQuantity, ImmutableList<Skill.Requirement> requirements)
            {
                Time = time;
                ProductType = productType;
                ProductQuantity = productQuantity;
                Requirements = requirements;
            }
        }
    }
}
