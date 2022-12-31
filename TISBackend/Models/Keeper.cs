namespace TISBackend.Models
{
    public class Keeper : Person
    {
        public int GrossWage { get; set; }
        public Keeper Supervisor { get; set; }
    }
}
