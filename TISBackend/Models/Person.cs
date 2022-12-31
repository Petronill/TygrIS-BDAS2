namespace TISBackend.Models
{
    public class Person
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        //PIN - Personal Identification number - aka Rodné číslo
        public string PIN { get; set; }
        public int? PhoneNumber { get; set; }
        public string Email { get; set; }
        public int? AccountNumber { get; set; }
        public Address Address { get; set; }
        public Document Photo { get; set; }
    }
}
