using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Models;

namespace Infrastructure.Interfaces
{
    public interface ILogFileService
    {
        public IEnumerable<LogFileInfo> GetLogFileInfos(string projectName);

        public void ImportAllLogs(string projectName);

        public void ImportLog(string projectName, string fileName);
    }
}
