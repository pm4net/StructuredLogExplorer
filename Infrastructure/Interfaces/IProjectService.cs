using Infrastructure.Models;

namespace Infrastructure.Interfaces
{
    public interface IProjectService
    {
        IEnumerable<Project> GetAvailableProjects();

        Project GetActiveProject();

        void AddProject(Project project);

        void DeleteProject(Project project);
    }
}
