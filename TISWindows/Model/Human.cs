using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TISWindows.Model
{
    internal class Human
    {

        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        //PIN - Personal Identification number - aka Rodné číslo
        public string? PIN { get; set; }
        public int? AccountNumber { get; set; }
        public int PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Image Photo { get; set; }
        public Address Address { get; set; }
        public Title? Title { get; set; }
    }
}
