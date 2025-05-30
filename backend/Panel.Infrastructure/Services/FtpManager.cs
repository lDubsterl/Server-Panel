﻿using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using System.Security.Cryptography;
using System.Text;

namespace Panel.Infrastructure.Services
{
	public class FtpManager: IFtpManager
	{
		private IOsInteraction _processManager;

		public FtpManager(IOsInteraction processManager)
		{
			_processManager = processManager;
		}

		public string ManageFTPUser(string username, ManageMode mode)
		{
			var modeString = mode == ManageMode.Create ? "-b -d" : "-D";
			var password = PasswordBuilder.GeneratePassword(12);
			_processManager.ExecuteCommand($"htpasswd -i {modeString} /etc/vsftpwd {username} {password}");
			return password;
		}
	}
}
