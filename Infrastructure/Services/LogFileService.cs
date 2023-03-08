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

            var logFiles = GetFileInfos(projectInfo?.LogDirectory ?? string.Empty);

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
            var notYetAdded = logFiles
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

        public IDictionary<string, LogFileInfo?> ImportAllLogs(string projectName)
        {
            var db = _projectService.GetProjectDatabase(projectName, readOnly: true);
            var projectInfo = ProjectInfoHelper.GetProjectInformation(db);
            var dict = new Dictionary<string, LogFileInfo?>();

            var logFiles = GetLogFileInfos(projectName);
            foreach (var logFile in logFiles)
            {
                dict.Add(logFile.Name, logFile);

                // File doesn't exist anymore
                if (!File.Exists(Path.Combine(projectInfo.LogDirectory, logFile.Name)))
                    continue;

                // Newest version of file was already imported
                if (logFile.LastImported is not null && logFile.LastChanged is not null && logFile.LastChanged < logFile.LastImported)
                    continue;

                var info = ImportLog(projectName, logFile.Name);
                dict[logFile.Name] = info;
            }

            return dict;
        }

        public LogFileInfo? ImportLog(string projectName, string fileName)
        {
            var db = _projectService.GetProjectDatabase(projectName, readOnly: false);
            var logDir = ProjectInfoHelper.GetProjectInformation(db).LogDirectory;
            var filePath = Path.Combine(logDir, fileName);
            var fileInfo = new FileInfo(filePath);
            var importTime = DateTime.Now;

            if (fileInfo.Exists)
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
                        var jsonLog = OcelJson.Deserialize(json, true);
                        OcelLiteDB.Serialize(db, jsonLog, true);
                        eventsAfter = jsonLog.Events.Count;
                        objectsAfter = jsonLog.Objects.Count;
                        break;
                    case OcelFileExtensions.Xml:
                        var xml = File.ReadAllText(filePath);
                        var xmlLog = OcelXml.Deserialize(xml);
                        OcelLiteDB.Serialize(db, xmlLog, true);
                        eventsAfter = xmlLog.Events.Count;
                        objectsAfter = xmlLog.Objects.Count;
                        break;
                    case OcelFileExtensions.LiteDb:
                        using (var logDb = new LiteDatabase($"Filename={filePath};ReadOnly=true"))
                        {
                            var log = OcelLiteDB.Deserialize(logDb);
                            OcelLiteDB.Serialize(db, log, true);
                            eventsAfter = log.Events.Count;
                            objectsAfter = log.Objects.Count;
                            logDb.Dispose();
                        }
                        break;
                }

                if (dbLogFile != null)
                {
                    if (dbLogFile.NoOfImportedEvents < eventsAfter)
                    {
                        dbLogFile.NoOfImportedEvents += eventsAfter - eventsBefore;
                    }

                    if (dbLogFile.NoOfImportedObjects < objectsAfter)
                    {
                        dbLogFile.NoOfImportedObjects += objectsAfter - objectsBefore;
                    }

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
                        LastImported = importTime
                    };

                    dbLogFileColl?.Insert(newDbLogFile);
                }

                return new LogFileInfo
                {
                    Name = fileName,
                    FileSize = fileInfo.Length,
                    LastChanged = fileInfo.LastWriteTime,
                    LastImported = importTime,
                    NoOfImportedEvents = eventsAfter,
                    NoOfImportedObjects = objectsAfter
                };
            }

            return null;
        }

        private static IList<FileInfo> GetFileInfos(string directory)
        {
            var logDir = new DirectoryInfo(directory);
            return logDir
                .EnumerateFiles(string.Empty, SearchOption.TopDirectoryOnly)
                .Where(f => f.Extension is OcelFileExtensions.LiteDb or OcelFileExtensions.Json or OcelFileExtensions.Xml)
                .ToList();
        }
    }
}
