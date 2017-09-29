using System.Collections.Immutable;
using System.Diagnostics;

namespace RedHill.Core.ESI
{
    [DebuggerDisplay("{Name}")]
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
    }
}