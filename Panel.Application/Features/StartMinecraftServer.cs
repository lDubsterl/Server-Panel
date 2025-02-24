using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Shared;
using System.Diagnostics;
using System.Text;

namespace Panel.Application.Features
{
	public class StartMinecraftServer(int id) : IRequest<IActionResult>
	{
		public int Id { get; } = id;
	}

	public class StartMinecraftServerHandler : IRequestHandler<StartMinecraftServer, IActionResult>
	{
		IConsoleHub _hub;
		IConfiguration _config;
		IOsInteractionsService _processManager;
		IUnitOfWork _unitOfWork;
		ILogger<StartMinecraftServerHandler> _logger;

		public StartMinecraftServerHandler(IConsoleHub hub, IConfiguration config, IOsInteractionsService processManager, IUnitOfWork unitOfWork, ILogger<StartMinecraftServerHandler> logger)
		{
			_hub = hub;
			_config = config;
			_processManager = processManager;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task<IActionResult> Handle(StartMinecraftServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id);

			if (user is null || user.MinecraftServerExecutable == "")
				return new BadRequestObjectResult(new BaseResponse(false, "There are no existing servers to run"));

			var serverRecord = await _unitOfWork.Repository<RunningServer>().Entities.FirstOrDefaultAsync(e => e.UserId == request.Id);
			var username = user.Email.Replace("@", "");
			var serverDirectory = $"{_config["ServersDirectory"]}/{username}/Minecraft/";
			Process serverProcess = _processManager.CreateCmdProcess($"docker run --rm -P -v {serverDirectory}:/usr --name {username} --entrypoint /opt/java/openjdk/bin/java minecraft {user.MinecraftServerExecutable}", serverDirectory);

			serverProcess.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					_logger.LogInformation(args.Data);
					//await _hub.Send(ServerTypes.Minecraft.ToString(), args.Data + "\n", request.Id);
				}
			};

			serverProcess.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					_logger.LogInformation(args.Data);
				}
			};
			serverProcess.Start();
			serverProcess.BeginErrorReadLine();
			serverProcess.BeginOutputReadLine();

			var server = new RunningServer
			{
				UserId = user.Id,
				ContainerName = username,
				ServerType = ServerTypes.Minecraft
			};
			int port;
			var commandResult = _processManager.ExecuteCommand($"docker port {username}");
			port = int.TryParse(UTF8Encoding.UTF8.GetBytes(commandResult.Split(':')[^1]), out port) ? port : 0;
			server.Port = port;

			await _unitOfWork.Repository<RunningServer>().AddAsync(server);
			await _unitOfWork.Save();

			_ = Task.Run(async () =>
			{
				await serverProcess.WaitForExitAsync();
				await _unitOfWork.Repository<RunningServer>().DeleteAsync(server);
				await _unitOfWork.Save();
			});

			return new OkObjectResult(new
			{
				isSuccess = true,
				message = "Server is starting...",
				port = server.Port,
			});
		}
	}
}
