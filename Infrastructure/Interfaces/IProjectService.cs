using Infrastructure.Models;

namespace Infrastructure.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAvailableProjects();

        Task<Project> GetActiveProject();

        Task AddProject(Project project);

        Task DeleteProject(Project project);
    }
}
