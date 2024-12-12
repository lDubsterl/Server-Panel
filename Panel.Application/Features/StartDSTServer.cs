using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.Features
{
	public class StartDSTServer(int id): ClientId(id), IRequest<IActionResult>
	{
	}

	public class StartDSTServerHandler : IRequestHandler<StartDSTServer, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IProcessManager _processManager;
		IConfiguration _configuration;
		IConsoleHub _hub;

		public StartDSTServerHandler(IUnitOfWork unitOfWork, IProcessManager processManager, IConfiguration configuration, IConsoleHub consoleHub)
		{
			_unitOfWork = unitOfWork;
			_processManager = processManager;
			_configuration = configuration;
			_hub = consoleHub;
		}

		public async Task<IActionResult> Handle(StartDSTServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<UserAccount>("Site accounts").GetByIdAsync(request.Id);

			var serverDirectory = $"{_configuration["DST_CLI_Directory"]}Don't Starve Together Dedicated Server\\bin\\";

			Process masterProcess = _processManager.CreateCmdProcess($"{serverDirectory}Server_Master{user.Email}.bat");
			Process cavesProcess = _processManager.CreateCmdProcess($"{serverDirectory}Server_Caves{user.Email}.bat");

			masterProcess.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(args.Data);
					await _hub.Send(ConsoleTypes.DstMaster.ToString(), args.Data + "\n", request.Id);
				}
			};

			masterProcess.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(ConsoleTypes.DstMaster.ToString(), "ERROR: " + args.Data);
					await _hub.Send(ConsoleTypes.DstMaster.ToString(), "ERROR: " + args.Data + "\n", request.Id);
				}
			};

			cavesProcess.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(args.Data);
					await _hub.Send(ConsoleTypes.DstCaves.ToString(), args.Data + "\n", request.Id);
				}
			};

			cavesProcess.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(ConsoleTypes.DstCaves.ToString(), "ERROR: " + args.Data);
					await _hub.Send(ConsoleTypes.DstCaves.ToString(), "ERROR: " + args.Data + "\n", request.Id);
				}
			};

			masterProcess.Start();
			cavesProcess.Start();
			masterProcess.BeginErrorReadLine();
			masterProcess.BeginOutputReadLine();
			cavesProcess.BeginErrorReadLine();
			cavesProcess.BeginOutputReadLine();

			var server = new RunningServer
			{
				UserId = user.Id,
				ServerProcessId = masterProcess.Id,
				ServerType = ConsoleTypes.DstMaster
			};
			await _unitOfWork.Repository<RunningServer>("Servers").AddAsync(server);

			server = new RunningServer
			{
				UserId = user.Id,
				ServerProcessId = cavesProcess.Id,
				ServerType = ConsoleTypes.DstCaves,
			};
			await _unitOfWork.Repository<RunningServer>("Servers").AddAsync(server);

			return new OkObjectResult("Server starting...");
		}
	}
}
