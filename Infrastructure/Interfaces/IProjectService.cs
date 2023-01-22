using Infrastructure.Models;
using LiteDB;

namespace Infrastructure.Interfaces
{
    public interface IProjectService
    {
        IEnumerable<ProjectInfo> GetAvailableProjects();

        ILiteDatabase GetProjectDatabase(string projectName, bool readOnly);

        void CreateProject(string projectName, string logDirectory);

        void CloseProject(string projectName);

        void DeleteProject(string projectName);
    }
}
