namespace TISModelLibrary
{
    public class Person
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        //PIN - Personal Identification number - aka Rodné číslo
        public long PIN { get; set; }
        public long? PhoneNumber { get; set; }
        public string Email { get; set; }
        public long? AccountNumber { get; set; }
        public Address Address { get; set; }
        public PersonalRoles Role { get; set; }
        public Document Photo { get; set; }

    }
}
