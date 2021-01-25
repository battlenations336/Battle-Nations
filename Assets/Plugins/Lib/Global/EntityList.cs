namespace BNR
{
    public class EntityListEntry
    {
        public EntityType Type { get; set; }
        public string Name { get; set; }

        public EntityListEntry(EntityType _type, string _name)
        {
            Type = _type;
            Name = _name;
        }
    }
}
