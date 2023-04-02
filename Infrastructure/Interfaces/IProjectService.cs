using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace Infrastructure.Interfaces
{
    /// <summary>
    /// A service that handles the project database files and their connections.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Get a list of available projects to select, including some basic information about them that is read from the database.
        /// </summary>
        public IEnumerable<string> GetAvailableProjects();

        /// <summary>
        /// Get the shared database connection for a given project, and load it if necessary.
        /// </summary>
        /// <exception cref="ArgumentException">If the project does not exist.</exception>
        public ILiteDatabase GetProjectDatabase(string projectName);

        /// <summary>
        /// Create a new project, if it doesn't already exist.
        /// </summary>
        /// <exception cref="ArgumentException">If the project already exists.</exception>
        void CreateProject(string projectName, string logDirectory);

        /// <summary>
        /// Close a project's database connection and release the associated log from memory.
        /// </summary>
        void CloseProject(string projectName);

        /// <summary>
        /// Delete a project entirely from disk.
        /// </summary>
        void DeleteProject(string projectName);
    }
}
