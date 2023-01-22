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
        public ProjectInfo(string name, string logDirectory)
        {
            Name = name;
            LogDirectory = logDirectory;
        }

        public string Name { get; set; }

        public string LogDirectory { get; set; }

        [BsonIgnore]
        public bool IsLoaded { get; set; }
    }
}
