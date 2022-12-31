using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TISWindows.Model
{
    internal class Keeper : Human
    {
        public int GrossWage { get; set; }
        public int? SuperiorId { get; set; }
    }
}
