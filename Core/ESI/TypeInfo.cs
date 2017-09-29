using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RedHill.Core.ESI
{
    public interface IType
    {
        int Id { get; }
        string Name { get; }
    }

    public class TypeInfo : IType
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public decimal? Volume { get; }

        public TypeInfo(int id)
        {
            Id = id;
        }
        public TypeInfo(int id, string name, string description, decimal? volume)
            : this(id)
        {

            Name = name;
            Description = description;
            Volume = volume;
        }
    }
}