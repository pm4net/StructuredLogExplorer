using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Infrastructure.Models
{
    public class ProjectInfo
    {
        public ProjectInfo(string logDirectory)
        {
            LogDirectory = logDirectory;
        }

        public string LogDirectory { get; set; }

        [BsonIgnore]
        public string Name { get; set; } = string.Empty;

        [BsonIgnore]
        public int NoOfEvents { get; set; }

        [BsonIgnore]
        public int NoOfObjects { get; set; }
    }
}
