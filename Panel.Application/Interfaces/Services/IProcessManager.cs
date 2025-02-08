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
		public Process CreateCmdProcess(string cmdArguments);
		public Task<string> ExecuteCommandAsync(string command);
		public string ExecuteCommand(string command);
	}
}
