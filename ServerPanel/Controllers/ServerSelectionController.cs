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

namespace ServerPanel.Controllers
{

	[Route("api/[controller]/{id}")]
	[ApiController]
	public class ServerSelectionController : ControllerBase
	{

		UserAccount GetUser(int id)
		{
			string connectionString = "Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;";
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
				return db.Query<UserAccount>("select * from \"Site accounts\" where Id = @id", new { id }).First();
			}
		}

		string ExecuteCommand(string command, bool isContinuously = false)
		{
			Process process = new();
			ProcessStartInfo startInfo = new();
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = $"/C {command}";
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			process.StartInfo = startInfo;
			process.Start();
			if (isContinuously)
			{
				string oldResult = "";
				while (!process.HasExited)
				{
					Thread.Sleep(2000);
					string cmdResult = process.StandardOutput.ReadLine();
					if (cmdResult != oldResult)
						Console.WriteLine(cmdResult);
					oldResult = cmdResult;
				}
			}
			var executingResult = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			return executingResult;
		}

		[Authorize, HttpPut("create/")]
		public StatusCodeResult CreateServer(int id, char serverLiteral)
		{
			using (IDbConnection db = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;"))
			{
				UserAccount accUser = db.Query<UserAccount>("select * from \"Site accounts\" where id = @id", new { id }).FirstOrDefault();
				var serverDirectory = $"cd /d d:\\Servers\\{accUser.Email} && ";
				ExecuteCommand($"cd /d d:\\Servers && mkdir {accUser.Email}");
				db.Execute("update \"Site accounts\" set minecraft = true where id = @id", id);
				ExecuteCommand(serverDirectory + "java -jar ../forge-1.20.4-49.0.33-installer.jar --installServer", true);
				ExecuteCommand(serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*", true);
				var eula = ExecuteCommand(serverDirectory + "type eula.txt");
				eula = eula.Replace("false", "true");
				var strings = eula.Split("\r\n");
				ExecuteCommand(serverDirectory + $"echo {strings[2]}> eula.txt");
			}
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		[Authorize, HttpDelete("delete/")]
		public StatusCodeResult DeleteServer(int id)
		{
			var user = GetUser(id);
			ExecuteCommand($"rmdir d:\\Servers\\{user.Email} /s/q");
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		public static Dictionary<int, Process> serverCmdProcesses = new Dictionary<int, Process>();
		private void PrintConsole(object cmdObj)
		{
			Process cmd = (Process)cmdObj;
			while (!cmd.HasExited)
			{
				Thread.Sleep(500);
				var text = cmd.StandardOutput.ReadLine();
				//Regex regex = new Regex("(\\w|\\/|\\.)\r\n(\\w|\\/|\\.)");
				//text = regex.Replace(text, $"$1$2");
				//regex = new Regex("\\u001b\\[K\\u001b\\[\\?25h\\u001b251|\\u001b\\[K|\\u001b\\[29;1H|\\u001b\\[28;1H|\\u001b\\[27;1H|" +
				//	"\\u001b\\[24;1H|\\u001b\\[\\?25h|\\u001b\\[25l");
				//text = regex.Replace(text, "");
				if (text != "")
					Console.WriteLine(text);
			}
		}

		[Authorize, HttpGet("panel/general/")]
		public string[] StartServer(int id)
		{
			var user = GetUser(id);
			var serverDirectory = $"cd /d d:\\Servers\\{user.Email} && ";
			Thread printingThread = new Thread(PrintConsole);
			Process process = new();
			ProcessStartInfo startInfo = new();
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/C "+ serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt nogui %*";
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardInput = true;
			process.StartInfo = startInfo;
			process.Start();
			serverCmdProcesses[id] = process;
			printingThread.Start(process);
			string directory = ExecuteCommand(serverDirectory + "dir /b");
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

		[Authorize, HttpPost("panel/general/")]
		public StatusCodeResult ExecuteServerCommand(int id, string command)
		{
			serverCmdProcesses[id].StandardInput.WriteLine(command);
			if (command == "stop")
			{
				serverCmdProcesses[id].WaitForExit();
				serverCmdProcesses.Remove(id);
			}
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}

		[Authorize, HttpGet("panel/files/")]
		public object ParseFile(int id, string name)
		{
			UserAccount user = GetUser(id);
			if (!name.Contains(@"d:\Servers\"))
				name = $"d:\\Servers\\{user.Email}\\" + name;
			if (System.IO.File.Exists(name))
			{
				return System.IO.File.ReadAllText(name);
			}
			if (System.IO.Directory.Exists(name))
			{
				var content = System.IO.Directory.GetFileSystemEntries(name);
				foreach (var str in content)
					str = str.Replace(@"\\", @"\");
			}
			return "";
		}

		[Authorize, HttpPatch("panel/files/")]
		public StatusCodeResult WriteFile(int id, string name, string fileContent)
		{
			UserAccount user = GetUser(id);
			var text = fileContent.Split("\\r\\n");
			System.IO.File.WriteAllLines($"d:\\Servers\\{user.Email}\\{name}", text);
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}
	}
}
