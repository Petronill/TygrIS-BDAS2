using System;

namespace TISBackend.Models
{
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //MaintCosts - how much for a  maintanance of the animal
        public int MaintCosts { get; set; }
        public DateTime Birth { get; set; }
        public DateTime Death { get; set; }
        public Species Species { get; set; }
        public Genus Genus { get; set; }
    }
}
