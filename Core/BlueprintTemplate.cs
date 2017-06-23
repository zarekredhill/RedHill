using System;
using System.Collections.Immutable;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public class BlueprintTemplate
    {
        public TypeInfo Type { get; }
        public ManufacturingDetails Manufacturing { get; }
        public CopyingDetails Copying { get; }

        public BlueprintTemplate(TypeInfo type, CopyingDetails copying, ManufacturingDetails manufacturing)
        {
            Type = type;
            Copying = copying;
            Manufacturing = manufacturing;
        }


        public class CopyingDetails
        {
            public TimeSpan Time { get; }
            public int MaxRunsPerCopy { get; }
            public ImmutableList<Skill.Requirement> Requirements { get; }
            public ImmutableDictionary<TypeInfo, int> Materials { get; }

            public CopyingDetails(TimeSpan time, int maxRunsPerCopy, ImmutableList<Skill.Requirement> requirements, ImmutableDictionary<TypeInfo, int> materials)
            {
                Time = time;
                MaxRunsPerCopy = maxRunsPerCopy;
                Requirements = requirements;
                Materials = materials;
            }
        }
        public class ManufacturingDetails
        {
            public TimeSpan Time { get; }
            public TypeInfo ProductType { get; }
            public int ProductQuantity { get; }
            public ImmutableList<Skill.Requirement> Requirements { get; }
            public ImmutableDictionary<TypeInfo, int> Materials { get; }

            public ManufacturingDetails(TimeSpan time, TypeInfo productType, int productQuantity, ImmutableList<Skill.Requirement> requirements, ImmutableDictionary<TypeInfo, int> materials)
            {
                Time = time;
                ProductType = productType;
                ProductQuantity = productQuantity;
                Requirements = requirements;
                Materials = materials;
            }
        }
    }
}
