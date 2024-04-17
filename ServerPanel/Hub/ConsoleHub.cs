using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ServerPanel
{
	public enum ConsoleType
	{
		Minecraft,
		DstMaster,
		DstCaves
	}
	public class ConsoleHub: Microsoft.AspNetCore.SignalR.Hub
	{
		public async Task Send(string message, string id, string consoleType)
		{
			await Clients.All.SendAsync("Send", message, id, consoleType);
		}
	}
}
