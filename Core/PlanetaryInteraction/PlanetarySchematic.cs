using System;
using RedHill.Core.ESI;

namespace RedHill.Core.PlanetaryInteraction
{
    public class PlanetarySchematic : IType
    {
        public int Id { get; }
        public string Name { get; }
        public TimeSpan CycleTime { get; }

        public PlanetarySchematic(int id, string name, TimeSpan cycleTime)
        {
            Id = id;
            Name = name;
            CycleTime = cycleTime;
        }
    }
}