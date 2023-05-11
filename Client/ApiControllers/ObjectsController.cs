using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OCEL.CSharp;
using pm4net.Utilities;
using StructuredLogExplorer.Models;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ObjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public IEnumerable<ObjectInfo> GetObjectTypeInfos(string projectName)
        {
            var db = _projectService.GetProjectDatabase(projectName);
            var log = OcelLiteDB.Deserialize(db);

            var infos = new Dictionary<string, ObjectInfo>();
            foreach (var @event in log.Events)
            {
                foreach (var objId in @event.Value.OMap)
                {
                    var obj = log.Objects[objId];
                    if (!infos.ContainsKey(obj.Type))
                    {
                        infos.Add(obj.Type, new ObjectInfo { Name = obj.Type });
                    }

                    infos[obj.Type].ReferencingEvents++;
                    infos[obj.Type].ObjectOccurrences.Add(new ObjectOccurrence
                    {
                        Activity = @event.Value.Activity,
                        Namespace = @event.Value.Namespace(),
                        SourceFile = @event.Value.SourceFile(),
                        LineNumber = @event.Value.LineNumber()
                    });

                    // TODO: Add code snippet if source file exists on local drive, and extract around line number if it exists
                }
            }

            // Merge duplicate occurrences
            foreach (var objectInfo in infos)
            {
                objectInfo.Value.ObjectOccurrences = objectInfo.Value.ObjectOccurrences.Distinct().ToList();
            }

            // Count how many different/unique instances of each object type there is
            foreach (var type in infos.Keys)
            {
                infos[type].UniqueInstances = log.Objects.Count(o => o.Value.Type == type);
            }

            return infos.Values.ToList();
        }
    }
}
