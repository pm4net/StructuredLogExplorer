using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using OCEL.CSharp;

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

        [HttpGet]
        [Route("getObjectTypeInfos")]
        public async Task<IEnumerable<ObjectInfo>> GetObjectTypeInfos(string projectName)
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

                    var occurrence = new ObjectOccurrence
                    {
                        Activity = @event.Value.Activity,
                        Namespace = @event.Value.Namespace(log) is OcelString ns ? ns.Value : null,
                        SourceFile = @event.Value.SourceFile(log) is OcelString sf ? sf.Value : null,
                        LineNumber = @event.Value.LineNumber(log) is OcelInteger ln ? ln.Value : null
                    };

                    if (!string.IsNullOrWhiteSpace(occurrence.SourceFile) && System.IO.File.Exists(occurrence.SourceFile))
                    {
                        var lines = await System.IO.File.ReadAllLinesAsync(occurrence.SourceFile);
                        if (occurrence.LineNumber != null)
                        {
                            const int linesBeforeAndAfter = 3;
                            var minIdx = (int) Math.Max(occurrence.LineNumber.Value - 1 - linesBeforeAndAfter, 0);
                            var maxIdx = (int) Math.Min(occurrence.LineNumber.Value + linesBeforeAndAfter, lines.Length - 1);
                            var linesToShow = lines[minIdx .. maxIdx];
                            occurrence.CodeSnippet = string.Join(Environment.NewLine, linesToShow);
                        }
                        else
                        {
                            occurrence.CodeSnippet = string.Join(Environment.NewLine, lines);
                        }
                    }

                    infos[obj.Type].ObjectOccurrences.Add(occurrence);
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

        [HttpPost]
        [Route("convertObjectsToAttributes")]
        public void ConvertObjectsToAttributes(string projectName, IEnumerable<string> objectTypes)
        {
            var db = _projectService.GetProjectDatabase(projectName);
            var log = OcelLiteDB.Deserialize(db);

            // Convert the object types to attributes
            var modifiedLog = log.ConvertObjectsToAttributes(objectTypes);

            // Clear existing log from database
            db.GetCollection("events").DeleteAll();
            db.GetCollection("objects").DeleteAll();
            db.GetCollection("global_attributes").DeleteAll();
            
            // Write modified log to database
            OcelLiteDB.Serialize(db, modifiedLog, false);
        }
    }
}
