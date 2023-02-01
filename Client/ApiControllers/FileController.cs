using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace StructuredLogExplorer.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogFileService _logFileService;

        public FileController(ILogFileService logFileService)
        {
            _logFileService = logFileService;
        }

        [HttpGet]
        [Route("logFileInfos")]
        public IEnumerable<LogFileInfo> GetLogFileInfos(string projectName)
        {
            return _logFileService.GetLogFileInfos(projectName);
        }

        [HttpPost]
        [Route("importAll")]
        public IDictionary<string, LogFileInfo?> ImportAll(string projectName)
        {
            return _logFileService.ImportAllLogs(projectName);
        }

        [HttpPost]
        [Route("importLog")]
        public LogFileInfo? ImportLog(string projectName, string fileName)
        {
            return _logFileService.ImportLog(projectName, fileName);
        }
    }
}
