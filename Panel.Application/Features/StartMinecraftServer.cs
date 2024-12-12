using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Panel.Application.Features
{
	public class StartMinecraftServer(int id) : ClientId(id), IRequest<IActionResult>
	{
	}

	public class StartMinecraftServerHandler : IRequestHandler<StartMinecraftServer, IActionResult>
	{
		IConsoleHub _hub;
		IConfiguration _config;
		IProcessManager _processManager;
		IUnitOfWork _unitOfWork;

		public StartMinecraftServerHandler(IConsoleHub hub, IConfiguration config, IProcessManager processManager, IUnitOfWork unitOfWork)
		{
			_hub = hub;
			_config = config;
			_processManager = processManager;
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Handle(StartMinecraftServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<UserAccount>("Site accounts").GetByIdAsync(request.Id);
			var serverRecord = await _unitOfWork.Repository<RunningServer>("Servers").GetByFieldAsync("Id", user.Id);

			if (user is null || !user.MinecraftServer)
				return new BadRequestObjectResult("There are no existing servers to run");

			var serverDirectory = _config["ServersDirectory"] + user.Email;
			Process process = _processManager.CreateCmdProcess("/C " + serverDirectory + "java @libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt nogui %*");

			process.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(args.Data);
					await _hub.Send(ConsoleTypes.MinecraftServer.ToString(), args.Data + "\n", request.Id);
				}
			};

			process.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(ConsoleTypes.MinecraftServer.ToString(), "ERROR: " + args.Data);
					await _hub.Send(ConsoleTypes.MinecraftServer.ToString(), "ERROR: " + args.Data + "\n", request.Id);
				}
			};
			process.Start();
			process.BeginErrorReadLine();
			process.BeginErrorReadLine();

			var server = new RunningServer
			{
				UserId = user.Id,
				ServerProcessId = process.Id,
				ServerType = ConsoleTypes.MinecraftServer
			};

			await _unitOfWork.Repository<RunningServer>("Servers").AddAsync(server);
			return new OkObjectResult("Server Starting...");
		}
	}
}
