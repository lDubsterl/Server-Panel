using Panel.Application.Interfaces.Services;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Panel.Infrastructure.Services
{
	public class OsInteractionsService : IOsInteractionsService
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
		private static int _potentiallyFreePort = 1025;
		public int FindFreePortInRange(int rangeStart, int rangeEnd)
		{
			int freePort = -1;
			rangeEnd = rangeEnd > 65535 ? 65535 : rangeEnd;
			if (_potentiallyFreePort >= rangeStart && _potentiallyFreePort <= rangeEnd)
				rangeStart = _potentiallyFreePort;
			for (int i = rangeStart; i <= rangeEnd; i++)
			{
				try
				{
					using (var tcpListener = new TcpListener(IPAddress.Loopback, i))
					{
						tcpListener.Start();
						tcpListener.Stop();
						freePort = i;
						_potentiallyFreePort = i + 1;
						break;
					}
				}
				catch (SocketException) { }
			}
			if (freePort == -1)
				return FindFreePortInRange(1025, 65535);
			return freePort;
		}
	}
}
