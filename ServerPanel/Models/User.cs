using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServerPanel.Models
{
	public class UserAccount
	{
		public int Id { get; set; }
		public string Email { get; set; }

		public string Password { get; set; }

		[JsonConstructor]
		public UserAccount(int id, string email, string password)
		{	
			Id = id;
			Email = email;
			Password = password;
		}
	}

	public class SshAccount
	{
		public string SshUsername { get; private set; }
		public string SshPassword { get; private set; }
		public bool MinecraftServer { get; set; }
		public bool DSTServer { get; set; }
		public SshAccount(string email)
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
		public SshAccount(string sshUsername, string sshPassword, bool minecraft, bool dst)
		{
			SshUsername = sshUsername;
			SshPassword = sshPassword;
			MinecraftServer = minecraft;
			DSTServer = dst;
		}
	}
}
