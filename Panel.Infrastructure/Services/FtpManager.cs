﻿using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using System.Security.Cryptography;

namespace Panel.Infrastructure.Services
{
	public class FtpManager: IFtpManager
	{
		private IOsInteractionsService _processManager;

		public FtpManager(IOsInteractionsService processManager)
		{
			_processManager = processManager;
		}

		public string ManageFTPUser(string username, ManageMode mode)
		{
			var modeString = mode == ManageMode.Create ? "-d" : "-D";
			var password = RandomNumberGenerator.GetBytes(12).ToString()!;
			_processManager.ExecuteCommand($"htpasswd -i {modeString} /etc/vsftpwd {username} {password}");
			return password;
		}
	}
}
