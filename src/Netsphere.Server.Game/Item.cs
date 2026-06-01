namespace Netsphere.Server.Game
{
    public class Item
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public ItemGender Gender { get; set; } = ItemGender.UNISEX;
    }
}
