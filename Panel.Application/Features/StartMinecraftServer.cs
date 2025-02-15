using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Shared;
using System.Diagnostics;

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
			var user = await _unitOfWork.Repository<UserAccount>().GetByIdAsync(request.Id);

			if (user is null || !user.MinecraftServer)
				return new BadRequestObjectResult(new BaseResponse(false, "There are no existing servers to run"));

			var serverRecord = await _unitOfWork.Repository<RunningServer>().Entities.FirstOrDefaultAsync(e => e.UserId == request.Id);
			var serverDirectory = _config["ServersDirectory"] + user.Email.Replace("@", "") + "/Minecraft/";
			Process serverProcess = _processManager.CreateCmdProcess("/C " + $"java {serverDirectory}@libraries/net/minecraftforge/forge/1.20.4-49.0.33/win_args.txt nogui %*");

			serverProcess.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(args.Data);
					await _hub.Send(ConsoleTypes.MinecraftServer.ToString(), args.Data + "\n", request.Id);
				}
			};

			var server = new RunningServer
			{
				UserId = user.Id,
				ServerProcessId = serverProcess.Id,
				ServerType = ConsoleTypes.MinecraftServer
			};

			serverProcess.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(ConsoleTypes.MinecraftServer.ToString(), "ERROR: " + args.Data);
				}
			};
			serverProcess.Start();
			serverProcess.BeginErrorReadLine();
			serverProcess.BeginErrorReadLine();

			await _unitOfWork.Repository<RunningServer>().AddAsync(server);
			await _unitOfWork.Save();

			_ = Task.Run(async () =>
			{
				await serverProcess.WaitForExitAsync();
				await _unitOfWork.Repository<RunningServer>().DeleteAsync(server);
				await _unitOfWork.Save();
			});

			return new OkObjectResult(new BaseResponse("Server starting..."));
		}
	}
}
