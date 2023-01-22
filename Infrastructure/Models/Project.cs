using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Project
    {
        public Project(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
