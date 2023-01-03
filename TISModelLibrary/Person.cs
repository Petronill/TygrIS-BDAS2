using System;

namespace TISModelLibrary
{
    public class Person
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //PIN - Personal Identification number - aka Rodné číslo
        public long PIN { get; set; }
        public long? PhoneNumber { get; set; }
        public string Email { get; set; }
        public long? AccountNumber { get; set; }
        public Address Address { get; set; }
        public PersonalRoles Role { get; set; }
        public Document Photo { get; set; }

        public DateTime Birthday()
        {
            string pin = PIN.ToString().PadLeft(10, '0');
            string yy = pin.Substring(0,2);
            string mm = pin.Substring(2,2);
            string dd = pin.Substring(4,2);

            char m = (mm[0] <= '1') ? mm[0] :
                    (mm[0] <= '4') ? (char)(mm[0] - 2) :
                    (mm[0] <= '6') ? (char)(mm[0] - 5) : (char)(mm[0] - 7);

            return DateTime.Parse($"{dd}. {m}{mm[1]}. {yy}");
        }
    }
}
