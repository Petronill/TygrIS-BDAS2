namespace TISModelLibrary
{
    public enum PersonalRoles
    {
        KEEPER, ADOPTER
    }

    public static class PersonalRoleUtils
    {
        public static PersonalRoles FromDbString(string abbr)
        {
            switch (abbr)
            {
                case "OSTR": return PersonalRoles.KEEPER;
                case "ADPT": return PersonalRoles.ADOPTER;
                default: return PersonalRoles.ADOPTER;
            }
        }

        public static string ToDbString(PersonalRoles role)
        {
            switch (role)
            {
                case PersonalRoles.KEEPER: return "OSTR";
                case PersonalRoles.ADOPTER: return "ADPT";
                default: return "";
            }
        }

        public static bool IsDbString(string abbr)
        {
            return abbr.Equals("OSTR") || abbr.Equals("ADPT");
        }
    }
}