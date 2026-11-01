namespace Vehicles.Core
{
    // Result of one vehicle in the report
    public class WorkResult
    {
        public string Name { get; set; } = "";
        public string State { get; set; } = "";   // Done / Error / Cancelled
        public string Result { get; set; } = "";  // result or error message
        public long DurationMs { get; set; }
    }
}
