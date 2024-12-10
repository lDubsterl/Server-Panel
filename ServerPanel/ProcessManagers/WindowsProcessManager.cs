using System.Diagnostics;
using System.Threading;
using System;

namespace ServerPanel.ProcessCreators
{
	public static class WindowsProcessManager
	{
		public static string ExecuteCommand(string command, bool isContinuously = false)
		{
			Process process = new();
			ProcessStartInfo startInfo = new()
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = $"/C {command}",
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			process.StartInfo = startInfo;
			process.Start();
			if (isContinuously)
			{
				string oldResult = "";
				while (!process.HasExited)
				{
					Thread.Sleep(100);
					string cmdResult = process.StandardOutput.ReadLine();
					if (cmdResult != oldResult)
						Console.WriteLine(cmdResult);
					oldResult = cmdResult;
				}
			}
			var executingResult = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			return executingResult;
		}

		public static Process CreateCmdProcess(string cmdArguments)
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
