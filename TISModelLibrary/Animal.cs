using System;

namespace TISModelLibrary
{
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Species Species { get; set; }
        public Sex Sex { get; set; }
        public Enclosure Enclosure { get; set; }
        //MaintCosts - how much for a  maintanance of the animal
        public int MaintCosts { get; set; }
        public DateTime Birth { get; set; }
        public DateTime? Death { get; set; }
        public Document Photo { get; set; }
    }
}
