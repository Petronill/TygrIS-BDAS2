using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TISWindows.Model
{
    internal class Animal
    {
        protected int Id { get; set; }
        protected string? Name { get; set; }
        //MaintCosts - how much for a  maintanance of the animal
        protected int MaintCosts { get; set; }
        protected DateTime Birth { get; set; }
        protected DateTime Death { get; set; }
    }
}
