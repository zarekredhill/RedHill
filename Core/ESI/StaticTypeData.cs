namespace RedHill.Core.ESI
{
    public struct StaticTypeData
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }

        public StaticTypeData(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}