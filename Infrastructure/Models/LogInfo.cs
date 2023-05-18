namespace Infrastructure.Models
{
    public class LogInfo
    {
        public IEnumerable<string> ObjectTypes { get; set; } = new List<string>();

        public string FirstEventTimestamp { get; set; } = string.Empty;

        public string LastEventTimestamp { get; set; } = string.Empty;
    }
}
