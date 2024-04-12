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
				return db.Query<UserAccount>("select sshUsername, sshPassword, Minecraft, DST from \"Site accounts\" where Id = @id", new { id }).First();
			}
		}

		string ExecuteCommand(string command, bool isContinuously = false)
		{
			Process process = new();
			ProcessStartInfo startInfo = new();
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = $"/C {command}";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo = startInfo;
			process.Start();
			if (isContinuously)
			{
				string cmdResult = "", oldResult = "";
				while (!process.HasExited)
				{
					Thread.Sleep(2000);
					cmdResult = process.StandardOutput.ReadToEnd();
					if (cmdResult != oldResult)
						Console.WriteLine(cmdResult);
					oldResult = cmdResult;
				}
			}
			process.WaitForExit();
			return process.StandardOutput.ReadToEnd();
		}

		[Authorize, HttpPut("create/")]
		public StatusCodeResult CreateServer(int id, char serverLiteral)
		{
			using (IDbConnection db = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;"))
			{
				UserAccount accUser = db.Query<UserAccount>("select * from \"Site accounts\" where id = @id", new { id }).FirstOrDefault();
				var serverDirectory = $"cd /d d:\\Servers\\{accUser.Email} && ";
				ExecuteCommand($"cd /d d:\\Servers && mkdir {accUser.Email}");
				db.Execute("update \"Ssh accounts\" set minecraft = true where id = @id", id);
				ExecuteCommand(serverDirectory + "java -jar../forge-1.20.4-49.0.33-installer.jar --installServer", true);
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
		private void PrintConsole(object obj)
		{
			if (obj is ThreadArguments executingCommand)
			{
				while (sessionInfo[executingCommand.id].client.IsConnected)
				{
					Thread.Sleep(500);
					var text = sessionInfo[executingCommand.id].stream.Read();
					//Regex regex = new Regex("(\\w|\\/|\\.)\r\n(\\w|\\/|\\.)");
					//text = regex.Replace(text, $"$1$2");
					//regex = new Regex("\\u001b\\[K\\u001b\\[\\?25h\\u001b251|\\u001b\\[K|\\u001b\\[29;1H|\\u001b\\[28;1H|\\u001b\\[27;1H|" +
					//	"\\u001b\\[24;1H|\\u001b\\[\\?25h|\\u001b\\[25l");
					//text = regex.Replace(text, "");
					if (text != "")
						Console.WriteLine(text);
				}
				sessionInfo[executingCommand.id].client.Disconnect();
			}
		}

		[Authorize, HttpGet("panel/general/")]
		public string[] StartServer(int id)
		{
			var user = GetUser(id);
			var serverDirectory = $"cd /d d:\\Servers\\{user.Email} && ";
			ShellStream shellStream = sshClient.CreateShellStream(string.Empty, 500, 0, 0, 0, 0);
			Thread printingThread = new Thread(PrintConsole);
			sessionInfo[id] = new SessionInfo(sshClient, shellStream);
			shellStream.WriteLine($"cd /d d:\\Servers\\{sshUser.SshUsername}");
			while (shellStream.Length != 0)
				Thread.Sleep(500);
			shellStream.WriteLine("java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt %*");
			while (shellStream.Length != 0)
				Thread.Sleep(500);
			printingThread.Start(new ThreadArguments(id, shellStream));
			string directory = sshClient.RunCommand(serverDirectory + "dir /b").Result;
			string[] foldersAndFiles = directory.Split("\r\n");
			return foldersAndFiles;
			return null;
		}

		[Authorize, HttpPost("panel/general/")]
		public StatusCodeResult ExecuteConsoleCommand(int id, string command)
		{
			if (sessionInfo[id].client.IsConnected)
			{
				sessionInfo[id].stream.WriteLine(command);
				if (command == "stop")
				{
					sessionInfo[id].client.Disconnect();
					sessionInfo[id].stream.Dispose();
					sessionInfo.Remove(id);
				}
				return new StatusCodeResult((int)HttpStatusCode.OK);
			}
			else
				return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
		}

		[Authorize, HttpGet("panel/files/")]
		public string ParseFile(int id, string name)
		{
			UserAccount user = GetUser(id);
			var fileContent = System.IO.File.ReadAllText($"d:\\Servers\\{user.Email}\\{name}");
			return fileContent;
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
