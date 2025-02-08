using Panel.Shared;

namespace Panel.Application.Interfaces.Services
{
	public interface IFtpManager
	{
		public string ManageFTPUser(string username, ManageMode mode);
	}
}
