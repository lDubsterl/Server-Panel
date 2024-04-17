using System.Threading.Tasks;

namespace ServerPanel.Hub
{
	public interface IMessageClient
	{
		public Task Send(string message, string id, string consoleType);
	}
}
