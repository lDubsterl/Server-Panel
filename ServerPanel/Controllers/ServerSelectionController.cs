using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Renci.SshNet;
using Dapper;
using Npgsql;

namespace ServerPanel.Controllers
{
	enum ServerType
	{
		TerrariaServer,
		MinecraftServer,
		DSTServer
	}
	[Route("[controller]")]
	[ApiController]
	public class ServerSelectionController : ControllerBase
	{
		string connectionString = "Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;";
		[HttpPost]
		public StatusCodeResult ConnectToServer(AccountUser user)
		{
			var client = new SshClient("localhost", "dubster", "anime");
			client.Connect();
			if (client.IsConnected)
			{
				using (IDbConnection db = new NpgsqlConnection(connectionString))
				{
					if (db.Query<SshUser>("select * from \"Ssh accounts\" where email = @email", new { user.Email }).Count() == 0)
					{
						var sshUser = new SshUser(user.Email);
						db.Execute("insert into \"Ssh accounts\" values(@email, @sshUsername, @sshPassword)", sshUser);
						client.RunCommand($"net user {sshUser.SshUsername} {sshUser.SshPassword} /add");
					}
				}
			}
			return Ok();
		}
	}
}
