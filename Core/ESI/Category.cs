using System.Collections.Immutable;

namespace RedHill.Core.ESI
{
    public class Category
    {
        public int Id { get; }
        public string Name { get; }
        public ImmutableList<int> GroupIds { get; }

        public Category(int id, string name, ImmutableList<int> groupIds)
        {
            Id = id;
            Name = name;
            GroupIds = groupIds;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}