using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.Interfaces.Services
{
	public interface IProcessManager
	{
		public string ExecuteCommand(string command);
		public Process CreateCmdProcess(string cmdArguments);

	}
}
