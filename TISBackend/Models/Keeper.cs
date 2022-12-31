namespace TISBackend.Models
{
    public class Keeper : Person
    {
        public int GrossWage { get; set; }
        public int? SupervisorId { get; set; }
    }
}
