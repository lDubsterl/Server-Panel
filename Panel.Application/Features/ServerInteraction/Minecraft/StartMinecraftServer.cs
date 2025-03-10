using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Shared;
using System.Diagnostics;
using System.Net.WebSockets;

namespace Panel.Application.Features.ServerInteraction.Minecraft
{
    public class StartMinecraftServer(int id) : IRequest<IActionResult>
    {
        public int Id { get; } = id;
    }

    public class StartMinecraftServerHandler : IRequestHandler<StartMinecraftServer, IActionResult>
    {
        IConfiguration _config;
        IOsInteractionsService _processManager;
        IUnitOfWork _unitOfWork;
        ILogger<StartMinecraftServerHandler> _logger;
        IServiceScopeFactory _scopeFactory;

		public StartMinecraftServerHandler(IConfiguration config, IOsInteractionsService processManager, 
            IUnitOfWork unitOfWork, ILogger<StartMinecraftServerHandler> logger, IServiceScopeFactory scopeFactory)
		{
			_config = config;
			_processManager = processManager;
			_unitOfWork = unitOfWork;
			_logger = logger;
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
            var serverProcess = _processManager.CreateCmdProcess($"docker run --rm -i -p {port}:25565 -v {serverDirectory}:/usr --name {username} --entrypoint /opt/java/openjdk/bin/java minecraft {user.MinecraftServerExecutable}");

            RunningServer server = new RunningServer
            {
                UserId = user.Id,
                ContainerName = username,
                ServerType = ServerTypes.Minecraft,
                Port = port
            };

            serverProcess.Start();
            //serverProcess.BeginErrorReadLine();
            //serverProcess.BeginOutputReadLine();
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
            await WaitForContainerAsync(username);
            return new OkObjectResult(new
            {
                isSuccess = true,
                message = "Server is starting...",
                address = _processManager.ExecuteCommand("curl ifconfig.me") + ":" + port,
                containerName = username
            });
        }
		private async Task<bool> WaitForContainerAsync(string containerName, int timeoutSeconds = 30)
		{
			var stopwatch = Stopwatch.StartNew();
			while (stopwatch.Elapsed < TimeSpan.FromSeconds(timeoutSeconds))
			{
				var result = _processManager.ExecuteCommand($"docker inspect -f '{{{{.State.Running}}}}' {containerName}");
				if (result.Trim() == "true")
					return true;
				await Task.Delay(500); // Подождем полсекунды перед повторной проверкой
			}
			return false;
		}
	}

}
