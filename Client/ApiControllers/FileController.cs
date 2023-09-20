using System.Net.Mime;
using System.Text;
using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using OCEL.CSharp;
using OCEL.Types;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogFileService _logFileService;
        private readonly IProjectService _projectService;
        private readonly IOutputCacheStore _outputCacheStore;

        public FileController(ILogFileService logFileService, IProjectService projectService, IOutputCacheStore outputCacheStore)
        {
            _logFileService = logFileService;
            _projectService = projectService;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [Route("logFileInfos")]
        public IEnumerable<LogFileInfo> GetLogFileInfos(string projectName)
        {
            return _logFileService.GetLogFileInfos(projectName);
        }

        [HttpPost]
        [Route("importAll")]
        public async Task<IDictionary<string, LogFileInfo?>> ImportAll(string projectName)
        {
            var logs = _logFileService.ImportAllLogs(projectName);
            await _outputCacheStore.EvictByTagAsync(CachePolicies.ObjectTypeInfo, CancellationToken.None);
            return logs;
        }

        [HttpPost]
        [Route("importLog")]
        public async Task<LogFileInfo?> ImportLog(string projectName, string fileName)
        {
            var log = _logFileService.ImportLog(projectName, fileName);
            await _outputCacheStore.EvictByTagAsync(CachePolicies.ObjectTypeInfo, CancellationToken.None);
            return log;
        }

        [HttpGet]
        [Route("exportOcel")]
        public IActionResult ExportOcel(string projectName, string format)
        {
            var db = _projectService.GetProjectDatabase(projectName);
            var log = OcelLiteDB.Deserialize(db);

            switch (format.ToLower())
            {
                case "json":
                    var json = OcelJson.Serialize(log, Formatting.Indented, false);
                    return File(Encoding.UTF8.GetBytes(json), MediaTypeNames.Application.Json, fileDownloadName: $"{projectName}.jsonocel");
                case "xml":
                    var xml = OcelXml.Serialize(log, Formatting.Indented, false);
                    return File(Encoding.UTF8.GetBytes(xml), MediaTypeNames.Application.Xml, fileDownloadName: $"{projectName}.xmlocel");
                case "litedb":
                    // Get temporary DB file
                    var tmpFile = Path.GetTempFileName();
                    var liteDb = new LiteDatabase(tmpFile);
                    // Write log to DB file
                    OcelLiteDB.Serialize(liteDb, log, false);
                    liteDb.Dispose();
                    // Create file stream that deletes the file once the stream is closed, and return it
                    var stream = new FileStream(tmpFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose);
                    return File(stream, MediaTypeNames.Application.Octet, fileDownloadName: $"{projectName}.db");
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), "Format not supported (use JSON, XML, or LiteDb)");
            }
        }
    }
}
