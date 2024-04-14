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
		public bool MinecraftServer { get; set; }
		public bool DSTServer { get; set; }

		[JsonConstructor]
		public UserAccount(int id, string email, string password)
		{	
			Id = id;
			Email = email;
			Password = password;
			MinecraftServer = DSTServer = false;
		}

		public UserAccount(int id, string email, string password, bool minecraft, bool dst)
		{
			Id = id;
			Email = email;
			Password = password;
			MinecraftServer = minecraft;
			DSTServer = dst;
		}
	}
}
