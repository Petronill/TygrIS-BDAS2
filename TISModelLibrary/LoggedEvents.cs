namespace TISModelLibrary
{
    public enum LoggedEvents
    {
        INSERT, UPDATE, DELETE, OTHER
    }

    public static class LoggedEventsUtils
    {
        public static LoggedEvents FromDbString(string source)
        {
            switch (source.ToLower())
            {
                case "insert": return LoggedEvents.INSERT;
                case "update": return LoggedEvents.UPDATE;
                case "delete": return LoggedEvents.DELETE;
                default: return LoggedEvents.OTHER;
            }
        }

        public static string ToDbString(LoggedEvents action)
        {
            switch (action) {
                case LoggedEvents.INSERT: return "insert";
                case LoggedEvents.UPDATE: return "update";
                case LoggedEvents.DELETE: return "delete";
                default: return "other";
            }
        }
    }
}
