using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;

namespace Infrastructure.Helpers
{
    public static class ProjectInfoHelper
    {
        public static ProjectInfo GetProjectInformation(string projectName, ILiteDatabase db)
        {
            var logDir = db.GetCollection<ProjectInfo>(nameof(ProjectInfo)).Query().FirstOrDefault()?.LogDirectory ?? string.Empty;
            return new ProjectInfo(logDir)
            {
                Name = projectName,
                NoOfEvents = db.GetCollection("events")?.Count() ?? default,
                NoOfObjects = db.GetCollection("objects")?.Count() ?? default
            };
        }
    }
}
