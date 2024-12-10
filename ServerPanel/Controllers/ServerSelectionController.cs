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
using System.Diagnostics;
using ServerPanel.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using ServerPanel.Hub;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

using WPM = ServerPanel.ProcessCreators.WindowsProcessManager;

namespace ServerPanel.Controllers
{

	[Route("api/[controller]/{id}/[action]")]
	[ApiController]
	public class ServerSelectionController : Controller
	{
		private IHubContext<ConsoleHub> _consoleHub;

		private NpgsqlConnection _db;

		private string _dstDedicatedServerRoot;
		private string _serversRoot;
		private IConfiguration _config;
		public ServerSelectionController(IConfiguration config, IHubContext<ConsoleHub> consoleHub, DbConnection db)
		{
			_dstDedicatedServerRoot = config["DST_CLI_Directory"];
			_serversRoot = config["ServersDirectory"];
			_config = config;
			_consoleHub = consoleHub;
			_db = db.connection;
		}
		UserAccount GetUser(int id)
		{
			string connectionString = "Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;";
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
				return db.Query<UserAccount>("select * from \"Site accounts\" where Id = @id", new { id }).First();
			}
		}

		[Authorize, HttpPut]
		public JsonResult CreateMinecraftServer(int id)
		{
			UserAccount accUser = _db.Query<UserAccount>("select * from \"Site accounts\" where id = @id", id).FirstOrDefault();
			if (accUser.MinecraftServer)
				return Json("Server is already created");
			var serverDirectory = _serversRoot + accUser.Email;
			ExecuteCommand($"cd /d {_serversRoot} && mkdir {accUser.Email}");
			_db.Execute("update \"Site accounts\" set minecraft = true where id = @id", id);
			ExecuteCommand(serverDirectory + "java -jar ../forge-1.20.4-49.0.33-installer.jar --installServer", true);
			ExecuteCommand(serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*", true);
			var eula = ExecuteCommand(serverDirectory + "type eula.txt");
			eula = eula.Replace("false", "true");
			var strings = eula.Split("\r\n");
			ExecuteCommand(serverDirectory + $"echo {strings[2]}> eula.txt");
			return Json("Created succesfully");
		}

		[Authorize, HttpPut]
		public JsonResult CreateDSTServer(int id, [Required] string serverName, string serverDesc, string password)
		{
			UserAccount accUser = _db.Query<UserAccount>("select * from \"Site accounts\" where id = @id", new { id }).FirstOrDefault();
			if (accUser.DSTServer)
				return Json("Server is already created");
			var serverDirectory = _config["DSTServerDirectory"];
			System.IO.Directory.CreateDirectory(serverDirectory + accUser.Email);
			System.IO.File.Copy(serverDirectory + "cluster_token.txt", serverDirectory + accUser.Email + "\\cluster_token.txt");
			serverDirectory += accUser.Email + "\\";
			System.IO.Directory.CreateDirectory(serverDirectory + "Master");
			System.IO.Directory.CreateDirectory(serverDirectory + "Caves");
			_db.Execute("update \"Site accounts\" set dst = true where id = @id", id);
			var ini = System.IO.File.ReadAllText($"{_serversRoot}DST templates\\master_server.txt");
			System.IO.File.WriteAllText(serverDirectory + "Master\\server.ini", ini);
			ini = System.IO.File.ReadAllText($"{_serversRoot}DST templates\\caves_server.txt");
			System.IO.File.WriteAllText(serverDirectory + "Caves\\server.ini", ini);
			ini = System.IO.File.ReadAllText($"{_serversRoot}DST templates\\caves_worldgen.txt");
			System.IO.File.WriteAllText(serverDirectory + "Caves\\worldgenoverride.lua", ini);
			var cluster = System.IO.File.ReadAllLines($"{_serversRoot}DST templates\\cluster_template.txt");
			cluster[13] = $"cluster_name = {serverName}";
			cluster[15] = $"cluster_description = {serverDesc}";
			cluster[19] = $"cluster_password = {password}";
			System.IO.File.WriteAllLines(serverDirectory + "cluster.ini", cluster);
			var server = System.IO.File.ReadAllText($"{_serversRoot}DST templates\\Server_Master.txt");
			server = server.Replace("MyDediServer", accUser.Email);
			System.IO.File.WriteAllText($"{_dstDedicatedServerRoot}Don't Starve Together Dedicated Server\\bin\\Server_Master{accUser.Email}.bat", server);
			server = System.IO.File.ReadAllText($"{_serversRoot}DST templates\\Server_Caves.txt");
			server = server.Replace("MyDediServer", accUser.Email);
			System.IO.File.WriteAllText(@$"{_dstDedicatedServerRoot}Don't Starve Together Dedicated Server\\bin\\Server_Caves{accUser.Email}.bat", server);
			return Json("Created succesfully");
		}

		[Authorize, HttpDelete]
		public StatusCodeResult DeleteMinecraftServer(int id)
		{
			var user = GetUser(id);
			System.IO.Directory.Delete(_serversRoot + user.Email, true);
			_db.Execute("update \"Site accounts\" set minecraft = false where id = @id", id);
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		[Authorize, HttpDelete]
		public StatusCodeResult DeleteDSTServer(int id)
		{
			var user = GetUser(id);

			System.IO.Directory.Delete(_config["DSTServerDirectory"] + user.Email, true);
			System.IO.File.Delete(@$"{_dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Caves{user.Email}.bat");
			System.IO.File.Delete(@$"{_dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Master{user.Email}.bat");
			_db.Execute("update \"Site accounts\" set dst = false where id = @id", id);
			dstServerProcesses.Remove(id);
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		private static Dictionary<int, Process> minecraftServerProcesses = new Dictionary<int, Process>();
		private static Dictionary<int, List<Process>> dstServerProcesses = new Dictionary<int, List<Process>>();

		class TextSource
		{
			public Process process;
			public int id;
			public ConsoleType consoleType;

			public TextSource(Process process, int id, ConsoleType consoleType)
			{
				this.process = process;
				this.id = id;
				this.consoleType = consoleType;
			}
		}
		private void PrintConsole(object textSource)
		{
			if (textSource is TextSource source)
			{
				var cmd = source.process;
				while (!cmd.HasExited)
				{
					var text = cmd.StandardOutput.ReadLine();
					if (text != "")
					{
						Console.WriteLine(text);
						_consoleHub.Clients.All.SendAsync("Send", text + "\n", source.id.ToString(), source.consoleType.ToString());
					}
				}
			}
		}

		[Authorize, HttpGet]
		public string[] StartMinecraftServer(int id)
		{
			var user = GetUser(id);
			var serverDirectory = _serversRoot + user.Email;
			Thread printingThread = new Thread(PrintConsole);
			Process process = WPM.CreateCmdProcess("/C " + serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt nogui %*");
			process.Start();
			minecraftServerProcesses[id] = process;
			printingThread.Start(new TextSource(process, id, ConsoleType.Minecraft));
			string directory = WPM.ExecuteCommand(serverDirectory + "dir /b");
			string[] allFiles = directory.Split("\r\n");
			LinkedList<string> formattedFoldersAndFiles = new();
			foreach (var filename in allFiles)
				if (!filename.Contains(".jar") && filename != "libraries")
					formattedFoldersAndFiles.AddLast(filename);
			string[] foldersAndFiles = new string[formattedFoldersAndFiles.Count];
			int i = 0;
			foreach (var filename in formattedFoldersAndFiles)
				foldersAndFiles[i++] = filename;
			return foldersAndFiles;
		}

		[Authorize, HttpGet]
		public StatusCodeResult StartDSTServer(int id)
		{
			var user = GetUser(id);
			var serverDirectory = $"{_dstDedicatedServerRoot}Don't Starve Together Dedicated Server\\bin\\";
			Thread masterPrintingThread = new Thread(PrintConsole);
			Thread cavesPrintingThread = new Thread(PrintConsole);
			Process process1 = CreateCmdProcess($"{serverDirectory}Server_Master{user.Email}.bat");
			Process process2 = CreateCmdProcess($"{serverDirectory}Server_Caves{user.Email}.bat");
			process1.Start();
			process2.Start();
			dstServerProcesses[id] = new List<Process>
			{
				process1,
				process2
			};
			masterPrintingThread.Start(new TextSource(process1, id, ConsoleType.DstMaster));
			cavesPrintingThread.Start(new TextSource(process2, id, ConsoleType.DstCaves));
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		[Authorize, HttpPost]
		public StatusCodeResult ExecuteMinecraftServerCommand(JObject obj)
		{
			int id = (int)obj.GetValue("id");
			string command = (string)obj.GetValue("command");
			var proc = minecraftServerProcesses.GetValueOrDefault(id);
			if (proc is null)
				return new StatusCodeResult((int)HttpStatusCode.Forbidden);
			proc.StandardInput.WriteLine(command);
			if (command == "stop")
			{
				proc.WaitForExit();
				minecraftServerProcesses.Remove(id);
			}
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		[Authorize, HttpPost]
		public StatusCodeResult ExecuteDSTServerCommand(int id, string command, bool isMaster)
		{
			Process proc;
			if (isMaster)
				proc = dstServerProcesses[id].First();
			else
				proc = dstServerProcesses[id].Last();
			proc.StandardInput.WriteLine(command);
			if (command.Contains("c_shutdown("))
			{
				proc.WaitForExit();
				dstServerProcesses.Remove(id);
			}
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		[Authorize, HttpGet]
		public object ParseFile(int id, [Required] string name)
		{
			UserAccount user = GetUser(id);
			name = $"{_serversRoot}{user.Email}\\" + name;
			if (System.IO.File.Exists(name))
			{
				return System.IO.File.ReadAllText(name);
			}
			if (System.IO.Directory.Exists(name))
			{
				var content = System.IO.Directory.GetFileSystemEntries(name);
				var formattedPathes = content;
				int i = 0;
				foreach (var str in content)
				{
					formattedPathes[i++] = str.Replace(@"\\", @"\");
				}
				return formattedPathes;
			}
			return "";
		}

		[Authorize, HttpPatch]
		public StatusCodeResult WriteFile(int id, [Required] string name, string fileContent)
		{
			UserAccount user = GetUser(id);
			var text = fileContent.Split("\\r\\n");
			System.IO.File.WriteAllLines($"{_serversRoot}{user.Email}\\{name}", text);
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}
	}
}
