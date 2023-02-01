using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Constants;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;
using OCEL.CSharp;

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
                .Where(f => f.Extension is OcelFileExtensions.LiteDb or OcelFileExtensions.Json or OcelFileExtensions.Xml)
                .ToList();

            // First get information about last updated date of file (if it still exists) 
            foreach (var fileInfo in dbLogFiles)
            {
                var logFile = logFiles?.FirstOrDefault(f => f.Name == fileInfo.Name);
                if (logFile != null)
                {
                    fileInfo.LastChanged = logFile.LastWriteTime;
                    fileInfo.FileSize = logFile.Length;
                }
            }

            // Fill list with new files that haven't been added yet
            IEnumerable<LogFileInfo>? notYetAdded = logFiles
                ?.Where(f => dbLogFiles.All(x => x.Name != f.Name))
                ?.Select(x => new LogFileInfo
                {
                    Name = x.Name,
                    LastChanged = x.LastWriteTime,
                    FileSize = x.Length
                });

            if (notYetAdded is not null)
            {
                dbLogFiles.AddRange(notYetAdded);
            }

            return dbLogFiles.OrderByDescending(x => x.LastChanged);
        }

        public void ImportAllLogs(string projectName)
        {
            throw new NotImplementedException();
        }

        public void ImportLog(string projectName, string fileName)
        {
            var db = _projectService.GetProjectDatabase(projectName, readOnly: false);
            var logDir = ProjectInfoHelper.GetProjectInformation(db).LogDirectory;
            var filePath = Path.Combine(logDir, fileName);

            if (File.Exists(filePath))
            {
                var dbLogFileColl = db.GetCollection<LogFileInfo>(Identifiers.LogFilesInfo);
                var dbLogFile = dbLogFileColl?.FindOne(x => x.Name == fileName);

                var eventsBefore = dbLogFile?.NoOfImportedEvents ?? default;
                var objectsBefore = dbLogFile?.NoOfImportedObjects ?? default;
                int eventsAfter, objectsAfter = eventsAfter = default;

                switch (Path.GetExtension(fileName))
                {
                    case OcelFileExtensions.Json:
                        var json = File.ReadAllText(filePath);
                        var jsonLog = OcelJson.Deserialize(json);
                        OcelLiteDB.Serialize(db, jsonLog);
                        eventsAfter = jsonLog.Events.Count;
                        objectsAfter = jsonLog.Objects.Count;
                        break;
                    case OcelFileExtensions.Xml:
                        var xml = File.ReadAllText(filePath);
                        var xmlLog = OcelXml.Deserialize(xml);
                        OcelLiteDB.Serialize(db, xmlLog);
                        eventsAfter = xmlLog.Events.Count;
                        objectsAfter = xmlLog.Objects.Count;
                        break;
                    case OcelFileExtensions.LiteDb:
                        using (var logDb = new LiteDatabase($"Filename={filePath};ReadOnly=true"))
                        {
                            var log = OcelLiteDB.Deserialize(logDb);
                            OcelLiteDB.Serialize(db, log);
                            eventsAfter = log.Events.Count;
                            objectsAfter = log.Objects.Count;
                            logDb.Dispose();
                        }
                        break;
                }

                if (dbLogFile != null)
                {
                    dbLogFile.NoOfImportedEvents += eventsAfter - eventsBefore;
                    dbLogFile.NoOfImportedObjects += objectsAfter - objectsBefore;
                    dbLogFile.LastImported = DateTime.Now;

                    dbLogFileColl?.Update(dbLogFile);
                }
                else
                {
                    var newDbLogFile = new LogFileInfo
                    {
                        Name = fileName,
                        NoOfImportedEvents = eventsAfter,
                        NoOfImportedObjects = objectsAfter,
                        LastImported = DateTime.Now
                    };

                    dbLogFileColl?.Insert(newDbLogFile);
                }

                _projectService.CloseProject(projectName); // To commit changes to the actual DB file
            }
        }
    }
}
