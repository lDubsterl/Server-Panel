using Microsoft.AspNetCore.SignalR;
using Panel.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace Panel.Infrastructure.Hubs
{
	public class ConsoleHub: Hub, IConsoleHub
	{
		public async Task Send(string consoleType, string message, int id)
		{
			await Clients.Client(id.ToString()).SendCoreAsync(consoleType, [message, consoleType]);
		}
	}
}
