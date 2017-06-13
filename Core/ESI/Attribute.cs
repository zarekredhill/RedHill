namespace RedHill.Core.ESI
{
    public class Attribute
    {
        public int Id { get; }
        public string Name { get; }

        public Attribute(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}