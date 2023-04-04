using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
	public enum LogLevel
	{
		Verbose,
		Debug,
		Information,
		Warning,
		Error,
		Fatal,
		Unknown
	}

	public static class LogLevelExtensions
	{
		public static LogLevel FromFSharpLogLevel(this pm4net.Types.LogLevel logLevel)
		{
			return (LogLevel) logLevel.Tag;
		}
	}
}
