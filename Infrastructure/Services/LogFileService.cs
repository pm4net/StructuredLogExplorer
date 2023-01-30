using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanBytes;
using Infrastructure.Constants;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;

namespace Infrastructure.Services
{
    public class LogFileService : ILogFileService
    {
        private readonly IProjectService _projectService;

        public LogFileService(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public IEnumerable<LogFileInfo> GetLogFileInfos(string projectName)
        {
            var db = _projectService.GetProjectDatabase(projectName, readOnly: true);
            var projectInfo = ProjectInfoHelper.GetProjectInformation(db);
            var dbLogFiles = db.GetCollection<LogFileInfo>(Identifiers.LogFilesInfo)?.FindAll()?.ToList();

            if (dbLogFiles is null)
                return new List<LogFileInfo>();

            var logDir = new DirectoryInfo(projectInfo.LogDirectory);
            List<FileInfo>? logFiles = logDir
                .EnumerateFiles(string.Empty, SearchOption.TopDirectoryOnly)
                .Where(f => f.Extension is ".db" or ".jsonocel" or ".xmlocel")
                .ToList();

            // First get information about last updated date of file (if it still exists) 
            foreach (var fileInfo in dbLogFiles)
            {
                var logFile = logFiles?.FirstOrDefault(f => f.Name == fileInfo.Name);
                if (logFile != null)
                {
                    fileInfo.LastChanged = logFile.LastWriteTime;
                    fileInfo.FileSize = logFile.Length.Bytes().ToString();
                }
            }

            // Fill list with new files that haven't been added yet
            IEnumerable<LogFileInfo>? notYetAdded = logFiles
                ?.Where(f => dbLogFiles.All(x => x.Name != f.Name))
                ?.Select(x => new LogFileInfo
                {
                    Name = x.Name,
                    LastChanged = x.LastWriteTime,
                    FileSize = x.Length.Bytes().ToString()
                });

            if (notYetAdded is not null)
            {
                dbLogFiles.AddRange(notYetAdded);
            }

            return dbLogFiles;
        }

        public void ImportAllLogs(string projectName)
        {
            throw new NotImplementedException();
        }

        public void ImportLog(string projectName, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
