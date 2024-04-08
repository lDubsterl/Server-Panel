using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using Renci.SshNet;
using Dapper;
using Npgsql;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace ServerPanel.Controllers
{
	enum ServerType
	{
		TerrariaServer,
		MinecraftServer,
		DSTServer
	}

	struct ThreadArguments
	{
		public int id;
		public ShellStream cmd;
		public ThreadArguments(int id, ShellStream command)
		{
			this.id = id;
			cmd = command;
		}
	}

	public struct SessionInfo
	{
		public SshClient client { get; set; }
		public ShellStream stream { get; set; }

		public SessionInfo(SshClient sshClient, ShellStream shellStream)
		{
			client = sshClient;
			stream = shellStream;
		}
	}
	[Route("[controller]")]
	[ApiController]
	public class ServerSelectionController : ControllerBase
	{
		string connectionString = "Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;";

		public static Dictionary<int, SessionInfo> sessionInfo = new Dictionary<int, SessionInfo>();

		[HttpPost("create/{id}")]
		public StatusCodeResult CreateServer(int id)
		{
			var client = new SshClient("localhost", "dubster", "anime");
			client.Connect();
			if (client.IsConnected)
			{
				using (IDbConnection db = new NpgsqlConnection(connectionString))
				{
					AccountUser accUser = db.Query<AccountUser>("select Email, Password from \"Site accounts\" where id = @id", new { id }).First();
					SshUser sshUser = null;
					try
					{
						sshUser = db.Query<SshUser>("select sshUsername, sshPassword from \"Ssh accounts\" where id = @id", new { id }).First();
					}
					catch (System.InvalidOperationException e)
					{
						Console.WriteLine(e.Message + id);
					}
					if (sshUser is null)
					{
						sshUser = new SshUser(accUser.Email);
						var serverDirectory = $"cd /d d:\\Servers\\{sshUser.SshUsername} && ";
						client.RunCommand($"cd /d d:\\Servers && mkdir {sshUser.SshUsername}");
						db.Execute("insert into \"Ssh accounts\"(SshUsername, SshPassword) values(@sshUsername, @sshPassword)", sshUser);
						client.RunCommand($"net user {sshUser.SshUsername} {sshUser.SshPassword} /add");
						//client.RunCommand("reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon\\SpecialAccounts\\UserList\"" +
						//	$" /t REG_DWORD /f /d 0 /v {sshUser.SshUsername}");
						client.RunCommand($"cd /d d:\\Servers && mkdir {sshUser.SshUsername}");
						var cmd = client.CreateCommand(serverDirectory + "java -jar ../forge-1.20.4-49.0.33-installer.jar --installServer");
						var exec = cmd.BeginExecute();
						string cmdResult = "", oldResult = "";
						while (!exec.IsCompleted)
						{
							Thread.Sleep(2000);
							cmdResult = cmd.Result;
							if (cmdResult != oldResult)
								Console.WriteLine(cmdResult);
							oldResult = cmdResult;
						}
						cmd = client.CreateCommand(serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*");
						exec = cmd.BeginExecute();
						while (!exec.IsCompleted)
						{
							Thread.Sleep(2000);
							cmdResult = cmd.Result;
							if (cmdResult != oldResult)
								Console.WriteLine(cmdResult);
							oldResult = cmdResult;
						}
						var eula = client.RunCommand(serverDirectory + "type eula.txt").Result;
						eula = eula.Replace("false", "true");
						var strings = eula.Split("\r\n");
						client.RunCommand(serverDirectory + $"echo {strings[2]}> eula.txt");
						client.Disconnect();
					}
				}
				return new StatusCodeResult((int)HttpStatusCode.OK);
			}
			return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
		}

		private void PrintConsole(object obj)
		{
			if (obj is ThreadArguments executingCommand)
			{
				while (sessionInfo[executingCommand.id].client.IsConnected)
				{
					Thread.Sleep(500);
					var text = sessionInfo[executingCommand.id].stream.Read();
					Regex regex = new Regex("(\\w|\\/|\\.)\r\n(\\w|\\/|\\.)");
					text = regex.Replace(text, $"$1$2");
					regex = new Regex("\\u001b\\[K\\u001b\\[\\?25h\\u001b251|\\u001b\\[K|\\u001b\\[29;1H|\\u001b\\[28;1H|\\u001b\\[27;1H|" +
						"\\u001b\\[24;1H|\\u001b\\[\\?25h|\\u001b\\[25l");
					text = regex.Replace(text, "");
					if (text != "")
						Console.WriteLine(text);
				}
				sessionInfo[executingCommand.id].client.Disconnect();
			}
		}

		[HttpPost("panel/general/{id}")]
		public string[] StartServer(int id)
		{
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
				SshUser user = db.Query<SshUser>("select sshUsername, sshPassword from \"Ssh accounts\" where Id = @id", new { id }).First();
				var sshClient = new SshClient("localhost", user.SshUsername, user.SshPassword);
				sshClient.Connect();
				if (sshClient.IsConnected)
				{
					var serverDirectory = $"cd /d d:\\Servers\\{user.SshUsername} && ";
					ShellStream shellStream = sshClient.CreateShellStream(string.Empty, 500, 0, 0, 0, 0);
					Thread printingThread = new Thread(PrintConsole);
					sessionInfo[id] = new SessionInfo(sshClient, shellStream);
					shellStream.WriteLine($"cd /d d:\\Servers\\{user.SshUsername}");
					while (shellStream.Length != 0)
						Thread.Sleep(500);
					shellStream.WriteLine("java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*");
					while (shellStream.Length != 0)
						Thread.Sleep(500);
					printingThread.Start(new ThreadArguments(id, shellStream));
					string directory = sshClient.RunCommand(serverDirectory + "dir /b").Result;
					string[] foldersAndFiles = directory.Split("\r\n");
					return foldersAndFiles;
				}
				return null;
			}
		}

		[HttpPost("panel/general/")]
		public StatusCodeResult ExecuteConsoleCommand(int id, string command)
		{
			if (sessionInfo[id].client.IsConnected)
			{
				sessionInfo[id].stream.WriteLine(command);
				if (command == "stop")
					sessionInfo[id].client.Disconnect();
				return new StatusCodeResult((int)HttpStatusCode.OK);
			}
			else
				return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
		}

		[HttpGet("panel/files/{id}")]
		public string ParseFile(int id, string name)
		{
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
				SshUser user = db.Query<SshUser>("select sshUsername, sshPassword from \"Ssh accounts\" where Id = @id", new { id }).First();
				var fileContent = System.IO.File.ReadAllText($"d:\\Servers\\{user.SshUsername}\\{name}");
				return fileContent;
			}
		}

		public class FileWritingInfo
		{
			public int Id;
			public string Name;
			public string Content;

			[JsonConstructor]
			public FileWritingInfo(int id, string name, string content)
			{
				Id = id;
				Name = name;
				Content = content;
			}
		}

		[HttpPost("panel/files/{id}")]
		public StatusCodeResult WriteFile(int id, string name, string fileContent)
		{
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
				SshUser user = db.Query<SshUser>("select sshUsername, sshPassword from \"Ssh accounts\" where Id = @id", new { id }).First();
				var text = fileContent.Split("\\r\\n");
				System.IO.File.WriteAllLines($"d:\\Servers\\{user.SshUsername}\\{name}", text);
				return new StatusCodeResult((int)HttpStatusCode.OK);
			}
		}
	}
}
