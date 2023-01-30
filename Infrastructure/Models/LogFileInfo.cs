using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Infrastructure.Models
{
    public class LogFileInfo
    {
        public string Name { get; set; } = string.Empty;

        public int NoOfImportedEvents { get; set; }

        public int NoOfImportedObjects { get; set; }

        public DateTime? LastImported { get; set; }

        [BsonIgnore]
        public DateTime? LastChanged { get; set; }
    }
}
