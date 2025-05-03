using System.Diagnostics;

namespace Panel.Application.Interfaces.Services
{
    public interface IOsInteraction
	{
		public Process CreateCmdProcess(string cmdArguments, string workingDirectory = "");
		public Task<string> ExecuteCommandAsync(string command, string workingDirectory = "");
		public string ExecuteCommand(string command, string workingDirectory = "");
		public int FindFreePortInRange(int rangeStart, int rangeEnd);
		public Task<bool> WaitForContainerAsync(string containerName, bool isStartingState, int timeoutSeconds = 10);
    }
}
