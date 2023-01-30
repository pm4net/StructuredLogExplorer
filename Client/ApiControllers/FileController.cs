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
    }
}
