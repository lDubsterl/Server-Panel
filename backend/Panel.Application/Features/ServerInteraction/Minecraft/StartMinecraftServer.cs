using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Diagnostics;

namespace Panel.Application.Features.ServerInteraction.Minecraft
{
	public class StartMinecraftServer(int id) : IRequest<IActionResult>
	{
		public int Id { get; } = id;
	}

	public class StartMinecraftServerHandler : IRequestHandler<StartMinecraftServer, IActionResult>
	{
		IConfiguration _config;
		IOsInteraction _processManager;
		IUnitOfWork _unitOfWork;
		IServiceScopeFactory _scopeFactory;
		public StartMinecraftServerHandler(IConfiguration config, IOsInteraction processManager,
			IUnitOfWork unitOfWork, IServiceScopeFactory scopeFactory)
		{
			_config = config;
			_processManager = processManager;
			_unitOfWork = unitOfWork;
			_scopeFactory = scopeFactory;
		}

		public async Task<IActionResult> Handle(StartMinecraftServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id);

			if (user is null || user.MinecraftServerExecutable == "")
				return new BadRequestObjectResult(new BaseResponse(false, "There are no existing servers to run"));

			var username = user.Email.Replace("@", "");
			var serverDirectory = $"{_config["ServersDirectory"]}/{username}/Minecraft/";
			int port = _processManager.FindFreePortInRange(1025, 65535);
			var serverProcess = _processManager.CreateCmdProcess($"docker run --rm -i -p {port}:25565 -v {serverDirectory}:/usr --name {username}Minecraft " +
				$"--entrypoint /opt/java/openjdk/bin/java minecraft {user.MinecraftServerExecutable}");

			RunningServer server = new RunningServer
			{
				UserId = user.Id,
				ContainerName = username + "Minecraft",
				ServerType = ServerTypes.Minecraft,
				Port = port
			};

			serverProcess.Start();
			serverProcess.BeginOutputReadLine();
			var serverTask = serverProcess.WaitForExitAsync();

			await _unitOfWork.Repository<RunningServer>().AddAsync(server);
			await _unitOfWork.Save();
			var serverId = server.Id;
			_ = Task.Run(async () =>
			{
				await serverTask;
				using var scope = _scopeFactory.CreateScope();
				var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var record = await scopedUnitOfWork.Repository<RunningServer>().GetByIdAsync(serverId);
				if (record != null)
				{
					await scopedUnitOfWork.Repository<RunningServer>().DeleteAsync(record);
					await scopedUnitOfWork.Save();
				}
			});
			await _processManager.WaitForContainerAsync(username + "Minecraft", true);
			return new OkObjectResult(new
			{
				isSuccess = true,
				message = "Server is starting...",
				address = _processManager.ExecuteCommand("curl ifconfig.me") + ":" + port,
				containerName = username + "Minecraft"
			});
		}
		
	}

}
