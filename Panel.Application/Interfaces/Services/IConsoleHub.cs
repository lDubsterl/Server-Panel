using System.Threading.Tasks;

namespace Panel.Application.Interfaces.Services
{
    public interface IConsoleHub
    {
        public Task Send(string consoleType, string message, int id);

	}
}
