using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;

namespace Infrastructure.Helpers
{
    public static class ProjectInfoHelper
    {
        public static ProjectInfo GetProjectInformation(ILiteDatabase db)
        {
            var projectInfo = db.GetCollection<ProjectInfo>(Identifiers.ProjectInfo).Query().FirstOrDefault();
            return new ProjectInfo(projectInfo?.Name ?? string.Empty, projectInfo?.LogDirectory ?? string.Empty)
            {
                NoOfEvents = db.GetCollection("events")?.Count() ?? default,
                NoOfObjects = db.GetCollection("objects")?.Count() ?? default
            };
        }
    }
}
