using Newtonsoft.Json;

namespace StructuredLogExplorer.Models
{
    public class ObjectInfo
    {
        [JsonProperty("id")]
        public string Name { get; set; } = string.Empty;

        public int Occurrences { get; set; }
    }
}
