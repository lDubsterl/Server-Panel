using Panel.Domain.Common;

namespace Panel.Application.Interfaces.Services
{
	public interface IFtpManager
	{
		public string ManageFTPUser(string username, ManageMode mode);
	}
}
