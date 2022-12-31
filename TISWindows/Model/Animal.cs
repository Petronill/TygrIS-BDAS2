using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TISWindows.Model
{
    internal class Animal
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        //MaintCosts - how much for a  maintanance of the animal
        public int MaintCosts { get; set; }
        public DateTime Birth { get; set; }
        public DateTime Death { get; set; }
        public Species? Species { get; set; }
        public Sex? Sex { get; set; }
        public Enclosure? Enclosure { get; set; }
        public Image? Photo { get; set; }

    }
}
