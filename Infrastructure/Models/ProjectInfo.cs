using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Newtonsoft.Json;

namespace Infrastructure.Models
{
    public class ProjectInfo
    {
		public ProjectInfo(string name, string logDirectory)
        {
            Name = name;
            LogDirectory = logDirectory;
        }

        [BsonId]
        public string Name { get; set; }

        public string LogDirectory { get; set; }
        
        /// <summary>
        /// When was the last time objects or attributes were converted?
        /// </summary>
        public DateTime? LastConversions { get; set; }

        [BsonIgnore]
        public int NoOfEvents { get; set; }

        [BsonIgnore]
        public int NoOfObjects { get; set; }
    }
}
