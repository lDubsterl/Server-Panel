using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServerPanel
{
	public class AccountUser
	{
		public string Email { get; set; }

		public string Password { get; set; }

		[JsonConstructor]
		public AccountUser(string email, string password)
		{
			Email = email;
			Password = password;
		}
	}

	public class SshUser
	{
		public string Email { get; set; }
		public string SshUsername { get; set; }
		public string SshPassword { get; set; }
		[JsonConstructor]
		public SshUser(string email)
		{
			Email = email;
			SshUsername = Email.Replace("@", "");

			string allowed = "1234567890QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm*-+#%";
			string password = "";
			Random rand = new();
			int passwordLength = rand.Next(8, 15);
			for (int i = 0; i < passwordLength; i++)
				password += allowed[rand.Next(67)];

			SshPassword = password;
		}

		public SshUser(string email, string sshUsername, string sshPassword)
		{
			Email = email;
			SshUsername = sshUsername;
			SshPassword = sshPassword;
		}
	}
}
