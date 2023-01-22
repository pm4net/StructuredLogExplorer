using Infrastructure.Interfaces;
using Infrastructure.Models;

namespace Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string _projectDirectory;
        private Project _activeProject;

        public ProjectService(string projectDirectory)
        {
            _projectDirectory = projectDirectory;

            // Ensure that the project directory exists
            Directory.CreateDirectory(projectDirectory);
        }

        public IEnumerable<Project> GetAvailableProjects()
        {
            var dirInfo = new DirectoryInfo(_projectDirectory);
            var files = dirInfo.EnumerateFiles().Where(f => f.Extension is "db");
            return files.Select(x => new Project(x.Name));
        }

        public Project GetActiveProject()
        {
            if (_activeProject != null)
            {
                return _activeProject;
            }

            return null;
        }

        public void AddProject(Project project)
        {
            
        }

        public void DeleteProject(Project project)
        {
            
        }
    }
}
