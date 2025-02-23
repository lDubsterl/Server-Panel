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
		public Process CreateCmdProcess(string cmdArguments, string workingDirectory = "");
		public Task<string> ExecuteCommandAsync(string command, string workingDirectory = "");
		public string ExecuteCommand(string command, string workingDirectory = "");
	}
}
