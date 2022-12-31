using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TISWindows.Model
{
    internal class Species
    {
        public int Id { get; set; }
        public Genus Genus { get; set; }
        public string? CzechName { get; set; }
        public string? LatinName { get; set; }
    }
}
