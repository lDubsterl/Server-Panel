using Microsoft.Web.Administration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Shared;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Panel.Infrastructure.Services
{
	public class FtpManager: IFtpManager
	{
		private IProcessManager _processManager;

		public FtpManager(IProcessManager processManager)
		{
			_processManager = processManager;
		}

		public string ManageFTPUser(string username, ManageMode mode)
		{
			var modeString = mode == 0 ? "-d" : "-D";
			var password = RandomNumberGenerator.GetBytes(12).ToString()!;
			_processManager.ExecuteCommand($"htpasswd -b {modeString} /etc/vsftpwd {username} {password}");
			return password;
		}
	}
}
