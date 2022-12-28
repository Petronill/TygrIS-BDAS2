using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TISWindows.Model
{
    internal class Human
    {

        protected int Id { get; set; }
        protected string? FirstName { get; set; }
        protected string? SecondName { get; set; }
        //PIN - Personal Identification number - aka Rodné číslo
        protected string? PIN { get; set; }
        protected int? AccountNumber { get; set; }
        protected int PhoneNumber { get; set; }
        protected string? Email { get; set; }
    }
}
