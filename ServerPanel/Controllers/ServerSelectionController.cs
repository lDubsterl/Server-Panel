using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Panel.Application.Features;
using Panel.Application.Interfaces.Services;
using Panel.Infrastructure.Hubs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ServerPanel.Controllers
{

	[Route("api/[controller]/{id}/[action]")]
	[ApiController, Authorize]
	public class ServerSelectionController : Controller
	{
		private readonly IConsoleHub _consoleHub;
		private readonly string _dstDedicatedServerRoot;
		private readonly string _serversRoot;
		private readonly IConfiguration _config;

		private readonly IMediator _mediator;
		public ServerSelectionController(IConfiguration config, ConsoleHub consoleHub, IMediator mediator)
		{
			_dstDedicatedServerRoot = config["DST_CLI_Directory"];
			_serversRoot = config["ServersDirectory"];
			_config = config;
			_consoleHub = consoleHub;
			_mediator = mediator;
		}

		[HttpPut]
		public async Task<IActionResult> CreateMinecraftServer(int id)
		{
			return await _mediator.Send(new CreateMinecraftServer(id));
		}

		[HttpPut]
		public async Task<IActionResult> CreateDSTServer(CreateDSTServer request)
		{
			return await _mediator.Send(request);
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteMinecraftServer(int id)
		{
			return await _mediator.Send(new DeleteMinecraftServer(id));
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteDSTServer(int id)
		{
			return await _mediator.Send(new DeleteDSTServer(id));
		}

		private static Dictionary<int, Process> minecraftServerProcesses = new Dictionary<int, Process>();
		private static Dictionary<int, List<Process>> dstServerProcesses = new Dictionary<int, List<Process>>();

		[HttpGet]
		public async Task<IActionResult> StartMinecraftServer(int id)
		{
			return await _mediator.Send(new StartMinecraftServer(id));
		}

		[HttpGet]
		public StatusCodeResult StartDSTServer(int id)
		{
			var user = GetUser(id);
			var serverDirectory = $"{_dstDedicatedServerRoot}Don't Starve Together Dedicated Server\\bin\\";
			Thread masterPrintingThread = new Thread(PrintConsole);
			Thread cavesPrintingThread = new Thread(PrintConsole);
			Process process1 = WPM.CreateCmdProcess($"{serverDirectory}Server_Master{user.Email}.bat");
			Process process2 = WPM.CreateCmdProcess($"{serverDirectory}Server_Caves{user.Email}.bat");
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

		[HttpPost]
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

		[HttpPost]
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

		[HttpGet]
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

		[HttpPatch]
		public StatusCodeResult WriteFile(int id, [Required] string name, string fileContent)
		{
			UserAccount user = GetUser(id);
			var text = fileContent.Split("\\r\\n");
			System.IO.File.WriteAllLines($"{_serversRoot}{user.Email}\\{name}", text);
			return new StatusCodeResult((int)HttpStatusCode.OK);
		}
	}
}
