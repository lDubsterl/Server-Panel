using Panel.Application.Interfaces.Services;
using System.Diagnostics;

namespace Panel.Infrastructure.ProcessManagers
{
    public class WindowsProcessManager : IProcessManager
	{
		IConsoleHub _hub;

		public WindowsProcessManager(IConsoleHub hub)
		{
			_hub = hub;
		}

		public string ExecuteCommand(string command)
		{
			string result;
			using (Process process = CreateCmdProcess($"/C {command}"))
			{
				process.Start();
				process.WaitForExit();
				result = process.StandardOutput.ReadToEnd();
			}
			return result;
		}

		public Process CreateCmdProcess(string cmdArguments)
		{
			Process proc = new();
			ProcessStartInfo startInfo = new()
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = cmdArguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardInput = true
			};
			proc.StartInfo = startInfo;

			return proc;
		}
	}
}
