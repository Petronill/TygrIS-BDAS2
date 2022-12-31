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
                default: return PersonalRoles.ADOPTER;
            }
        }
    }
}