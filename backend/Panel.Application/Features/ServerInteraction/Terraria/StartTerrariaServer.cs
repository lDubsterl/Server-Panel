using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction.Terraria
{
	public class StartTerrariaServer(int userId) : IRequest<IActionResult>
	{
		public int UserId { get; set; } = userId;
	}

	public class StartTerrariaServerHandler : IRequestHandler<StartTerrariaServer, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IOsInteraction _processManager;
		IConfiguration _configuration;
		IServiceScopeFactory _scopeFactory;
		ILogger<StartTerrariaServerHandler> _logger;

		public StartTerrariaServerHandler(IUnitOfWork unitOfWork, IOsInteraction processManager, IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<StartTerrariaServerHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_processManager = processManager;
			_configuration = configuration;
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		//public StartTerrariaServerHandler(IUnitOfWork unitOfWork, IOsInteraction processManager, IConfiguration configuration, IServiceScopeFactory scopeFactory)
		//{
		//	_unitOfWork = unitOfWork;
		//	_processManager = processManager;
		//	_configuration = configuration;
		//	_scopeFactory = scopeFactory;
		//}

		public async Task<IActionResult> Handle(StartTerrariaServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
			if (user == null) return new BadRequestObjectResult(new BaseResponse(false, "User not found"));
			if (!user.TerrariaServer) return new BadRequestObjectResult(new StatusCodeResult(StatusCodes.Status418ImATeapot));

			var username = user.Email.Replace("@", "");
			var serverDirectory = $"{_configuration["ServersDirectory"]}/{username}/Terraria";

			int port = _processManager.FindFreePortInRange(1025, 65535);
			var serverProcess = _processManager.CreateCmdProcess($"docker run -i --rm -p {port}:7777 -v {serverDirectory}:/home/tml/.local/share/Terraria/tModLoader" +
				$" --name {username}Terraria terraria-server:latest");

			RunningServer server = new RunningServer
			{
				UserId = user.Id,
				ContainerName = username + "Terraria",
				ServerType = ServerTypes.Terraria,
				Port = port
			};
			serverProcess.OutputDataReceived += (sender, e) =>
			{
				_logger.LogInformation(e.Data);
			};
			serverProcess.Start();
			serverProcess.BeginOutputReadLine();
			serverProcess.BeginErrorReadLine();
			var serverTask = serverProcess.WaitForExitAsync();
			if (!await _processManager.WaitForContainerAsync(username + "Terraria", true, 15))
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);

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

			return new OkObjectResult(new
			{
				isSuccess = true,
				message = "Server is starting...",
				address = _processManager.ExecuteCommand("curl ifconfig.me") + ":" + port,
				containerName = username + "Terraria"
			});
		}
	}
}
