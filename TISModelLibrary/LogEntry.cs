using System;

namespace TISModelLibrary
{
    public class LogEntry
    {
        public string Table { get; set; }
        public string Event { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }
}
