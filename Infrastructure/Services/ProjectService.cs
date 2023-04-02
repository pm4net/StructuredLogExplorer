using Infrastructure.Constants;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using LiteDB;

namespace Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string _projectDir;
        private Project? _activeProject;

        public ProjectService(string projectDir)
        {
            _projectDir = projectDir;

            // Ensure that the project directory exists
            Directory.CreateDirectory(projectDir);
        }

        public IEnumerable<string> GetAvailableProjects()
        {
            var dirInfo = new DirectoryInfo(_projectDir);
            var files = dirInfo.EnumerateFiles().Where(f => f.Extension is ".db" && !f.Name.EndsWith("-log.db"));
            return files.Select(f => Path.GetFileNameWithoutExtension(f.Name));
        }

        public ILiteDatabase GetProjectDatabase(string projectName)
        {
	        if (_activeProject?.Name == projectName && _activeProject?.Database != null)
	        {
				return _activeProject.Database;
			}
	        
            // Close existing connection
            _activeProject?.Database.Dispose();

            // Open new connection
            var fileName = GetDbFileName(projectName);
            _activeProject = new Project(projectName, new LiteDatabase(fileName));

	        return _activeProject.Database;
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
            _activeProject?.Database.Dispose();
            _activeProject = null;
        }

        public void DeleteProject(string projectName)
        {
            CloseProject(projectName);
            File.Delete(GetDbFileName(projectName));
        }

        /// <summary>
        /// Get the full path to the database file with the given name.
        /// </summary>
        private string GetDbFileName(string projectName)
        {
            return Path.Combine(_projectDir, $"{projectName}.db");
        }

        private class Project
        {
	        public Project(string name, ILiteDatabase database)
	        {
		        Name = name;
		        Database = database;
	        }

	        public string Name { get; set; }

	        public ILiteDatabase Database { get; set; }
        }
    }
}
