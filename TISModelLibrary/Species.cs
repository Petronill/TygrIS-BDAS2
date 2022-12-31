namespace TISModelLibrary
{
    public class Species
    {
        public int Id { get; set; }
        public string CzechName { get; set; }
        public string LatinName { get; set; }
        public Genus Genus { get; set; }
    }
}
