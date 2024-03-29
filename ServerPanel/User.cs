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
		public string SshUsername { get; set; }
		public string SshPassword { get; set; }
		public SshUser(string email)
		{
			SshUsername = email.Replace("@", "");

			string allowed = "1234567890QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm*-+#%";
			string password = "";
			Random rand = new();
			int passwordLength = rand.Next(8, 15);
			for (int i = 0; i < passwordLength; i++)
				password += allowed[rand.Next(67)];

			SshPassword = password;
		}

		[JsonConstructor]
		public SshUser(string sshUsername, string sshPassword)
		{
			SshUsername = sshUsername;
			SshPassword = sshPassword;
		}
	}
}
