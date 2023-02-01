using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;

namespace Infrastructure.Models
{
    public class LogFileInfo
    {
        [JsonPropertyName("id")]
        public string Name { get; set; } = string.Empty;

        public int NoOfImportedEvents { get; set; }

        public int NoOfImportedObjects { get; set; }

        [BsonIgnore]
        public long FileSize { get; set; }

        public DateTime? LastImported { get; set; }

        [BsonIgnore]
        public DateTime? LastChanged { get; set; }
    }
}
