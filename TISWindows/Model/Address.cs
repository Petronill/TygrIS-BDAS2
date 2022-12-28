using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TISWindows.Model
{
    internal class Address
    {
        protected int Id { get; set; }
        protected string? Street { get; set; }
        protected string? HouseNumber { get; set; }
        protected string? Cíty { get; set; }
        protected string? PostalCode { get; set; }
        protected string? Country { get; set; }

    }

}
