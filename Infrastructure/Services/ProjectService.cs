using Infrastructure.Interfaces;
using Infrastructure.Models;

namespace Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string _projectDirectory;

        public ProjectService(string projectDirectory)
        {
            _projectDirectory = projectDirectory;

            // Ensure that the project directory exists
            Directory.CreateDirectory(projectDirectory);
        }

        public async Task<IEnumerable<Project>> GetAvailableProjects()
        {
            return new List<Project>();
        }

        public async Task<Project> GetActiveProject()
        {
            return new Project("Test", "C:\\");
        }

        public async Task AddProject(Project project)
        {
            
        }

        public async Task DeleteProject(Project project)
        {
            
        }
    }
}
