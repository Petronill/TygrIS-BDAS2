namespace TISBackend.Models
{
    public class Enclosure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public Pavilion Pavilion { get; set; }
    }
}
