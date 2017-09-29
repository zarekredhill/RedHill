using System;
using System.Diagnostics;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    [DebuggerDisplay("{Name}")]
    public class PlanetaryCommodity : IType
    {
        public int Id { get; }
        public string Name { get; }
        public decimal Volume { get; }

        public PlanetaryCommodity(int id, string name, decimal volume)
        {
            Id = id;
            Name = name;
            Volume = volume;
        }
    }
}