using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;

namespace Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string _projectDirectory;
        private readonly IDictionary<string, ILiteDatabase> _projectDatabases;

        public ProjectService(string projectDirectory)
        {
            _projectDirectory = projectDirectory;
            _projectDatabases = new Dictionary<string, ILiteDatabase>();

            // Ensure that the project directory exists
            Directory.CreateDirectory(projectDirectory);
        }

        private string GetDbFileName(string projectName)
        {
            return Path.Combine(_projectDirectory, $"{projectName}.db");
        }

        public IEnumerable<ProjectInfo> GetAvailableProjects()
        {
            var dirInfo = new DirectoryInfo(_projectDirectory);
            IEnumerable<FileInfo> files = dirInfo.EnumerateFiles().Where(f => f.Extension is ".db");
            return files.Select(x =>
            {
                var name = Path.GetFileNameWithoutExtension(x.Name);
                var logDir = GetProjectDatabase(name, readOnly: true)
                    .GetCollection<ProjectInfo>(nameof(ProjectInfo))?
                    .Query()
                    .FirstOrDefault()?.LogDirectory;

                var projectInfo = new ProjectInfo(name, logDir ?? string.Empty)
                {
                    IsLoaded = _projectDatabases.ContainsKey(name)
                };
                return projectInfo;
            });
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
                // Remember to dispose after being done with it!
                return new LiteDatabase($"Filename={fileName};ReadOnly=true;");
            }

            if (_projectDatabases.ContainsKey(projectName))
            {
                return _projectDatabases[projectName];
            }

            var db = new LiteDatabase(fileName);
            _projectDatabases.Add(projectName, db);
            return db;
        }

        public void CreateProject(string projectName, string logDirectory)
        {
            var fileName = GetDbFileName(projectName);
            if (File.Exists(fileName))
                return;

            using var db = new LiteDatabase(fileName);

            ILiteCollection<ProjectInfo>? infoColl = db.GetCollection<ProjectInfo>(nameof(ProjectInfo));
            infoColl?.Insert(new ProjectInfo(projectName, logDirectory));

            db.Dispose();
        }

        public void CloseProject(string projectName)
        {
            if (_projectDatabases.ContainsKey(projectName))
            {
                _projectDatabases[projectName].Dispose();
            }
        }

        public void DeleteProject(string projectName)
        {
            CloseProject(projectName);
            File.Delete(Path.Combine(_projectDirectory, $"{projectName}.db"));
        }
    }
}
