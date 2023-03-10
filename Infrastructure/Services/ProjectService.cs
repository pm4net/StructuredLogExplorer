using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;
using OCEL.CSharp;

namespace Infrastructure.Services
{
    public class ProjectService : IProjectService, IDisposable
    {
        private readonly string _projectDir;
        private readonly IDictionary<string, ILiteDatabase> _dbConnections;
        private readonly IDictionary<string, (DateTime, OcelLog)> _logs;

        public ProjectService(string projectDir)
        {
            _projectDir = projectDir;
            _dbConnections = new Dictionary<string, ILiteDatabase>();
            _logs = new Dictionary<string, (DateTime, OcelLog)>();

            // Ensure that the project directory exists
            Directory.CreateDirectory(projectDir);
        }

        public IEnumerable<string> GetAvailableProjects()
        {
            var dirInfo = new DirectoryInfo(_projectDir);
            var files = dirInfo.EnumerateFiles().Where(f => f.Extension is ".db");
            return files.Select(f => Path.GetFileNameWithoutExtension(f.Name));
        }

        public ILiteDatabase GetProjectDatabase(string projectName, bool readOnly)
        {
            var fileName = GetDbFileName(projectName);
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("File does not exist", nameof(projectName));
            }

            if (readOnly)
            {
                return new LiteDatabase($"Filename={fileName};ReadOnly=true;");
            }

            _dbConnections.TryGetValue(projectName, out var db);
            if (db is not null)
            {
                return db;
            }

            db = new LiteDatabase($"Filename={fileName};Connection=shared;");
            _dbConnections.Add(projectName, db);
            return db;
        }

        public OcelLog GetProjectLog(string projectName)
        {
            var db = GetProjectDatabase(projectName, true);
            var files = db.GetCollection<LogFileInfo>(Identifiers.LogFilesInfo)?.FindAll()?.ToList();

            if (_logs.ContainsKey(projectName) && (!files?.Any(f => f.LastImported > _logs[projectName].Item1) ?? true))
            {
                return _logs[projectName].Item2;
            }

            var log = OcelLiteDB.Deserialize(db);
            _logs.Add(projectName, (DateTime.Now, log));
            db.Dispose();
            return log;
        }

        public void CreateProject(string projectName, string logDirectory)
        {
            var fileName = GetDbFileName(projectName);
            if (File.Exists(fileName))
                throw new ArgumentException("Project already exists", nameof(projectName));

            if (!Directory.Exists(logDirectory))
                throw new ArgumentException("Directory does not exist", nameof(logDirectory));

            using var db = new LiteDatabase(fileName);
            var infoColl = db.GetCollection<ProjectInfo>(Identifiers.ProjectInfo);
            infoColl?.Insert(new ProjectInfo(projectName, logDirectory));
            db.Dispose();
        }

        public void CloseProject(string projectName)
        {
            _dbConnections.TryGetValue(projectName, out var db);
            db?.Dispose();
            _dbConnections.Remove(projectName);
            _logs.Remove(projectName);
        }

        public void DeleteProject(string projectName)
        {
            CloseProject(projectName);
            File.Delete(GetDbFileName(projectName));
        }

        public void Dispose()
        {
            foreach (var key in _dbConnections.Keys)
            {
                CloseProject(key);
            }
        }

        /// <summary>
        /// Get the full path to the database file with the given name.
        /// </summary>
        private string GetDbFileName(string projectName)
        {
            return Path.Combine(_projectDir, $"{projectName}.db");
        }
    }
}
