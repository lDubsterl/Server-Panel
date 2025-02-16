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
		public string ExecuteCommand(string command)
		{
			string result;
			using (Process process = CreateCmdProcess(command))
			{
				process.Start();
				process.WaitForExit();
				result = process.StandardOutput.ReadToEnd();
			}
			return result;
		}
		public async Task<string> ExecuteCommandAsync(string command)
        {
            string result;
            using (Process process = CreateCmdProcess(command))
            {
                process.Start();
                await process.WaitForExitAsync();
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
                FileName = _shellName,
                Arguments = _baseArguments + $"{cmdArguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };
            proc.StartInfo = startInfo;

            return proc;
        }
    }
}
