using Microsoft.Extensions.Configuration;
using Microsoft.Web.Administration;
using Panel.Application.Interfaces.Services;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Panel.Infrastructure.Services
{
	public class ProcessManager: IProcessManager
	{
		private static string _shellName = "/bin/bash";
		private static string _baseArguments = "-c ";

		public string ExecuteCommand(string command, string workingDirectory = "")
		{
			string result;
			using (Process process = CreateCmdProcess(command, workingDirectory))
			{
				process.Start();
				process.WaitForExit();
				result = process.StandardOutput.ReadToEnd();
			}
			return result;
		}
		public async Task<string> ExecuteCommandAsync(string command, string workingDirectory = "")
		{
			string result;
			using (Process process = CreateCmdProcess(command, workingDirectory))
			{
				process.Start();
				await process.WaitForExitAsync();
				result = process.StandardOutput.ReadToEnd();
			}
			return result;
		}
		public Process CreateCmdProcess(string cmdArguments, string directory = "")
		{
			Process proc = new();
			ProcessStartInfo startInfo = new()
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = _shellName,
				WorkingDirectory = directory,
				Arguments = _baseArguments + $"\"{cmdArguments}\"",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true
			};
			proc.StartInfo = startInfo;

			return proc;
		}
	}
}
