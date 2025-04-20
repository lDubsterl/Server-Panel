using Docker.DotNet.Models;
using MediatR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Diagnostics;

namespace Panel.Application.Features.ServerInteraction.DST
{
	public class StartDSTServer(int id) : IRequest<IActionResult>
	{
		public int Id { get; } = id;
	}

	public class StartDSTServerHandler : IRequestHandler<StartDSTServer, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IOsInteraction _processManager;
		IConfiguration _configuration;
		IServiceScopeFactory _scopeFactory;

		public StartDSTServerHandler(IUnitOfWork unitOfWork, IOsInteraction processManager, IConfiguration configuration, IServiceScopeFactory scopeFactory)
		{
			_unitOfWork = unitOfWork;
			_processManager = processManager;
			_configuration = configuration;
			_scopeFactory = scopeFactory;
		}

		public async Task<IActionResult> Handle(StartDSTServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.Id);
			if (user == null) return new BadRequestObjectResult(new BaseResponse(false, "User not found"));
			if (!user.DSTServer) return new BadRequestObjectResult(new StatusCodeResult(StatusCodes.Status418ImATeapot));

			var username = user.Email.Replace("@", "");
			var serverDirectory = $"{_configuration["ServersDirectory"]}/{username}/";
			int[] ports = { -1, -1, -1, -1, -1 };
			for (int i = 0; i < ports.Length; i++)
				ports[i] = _processManager.FindFreePortInRange(1025, 65535);
			SetConfigPorts(serverDirectory + "DoNotStarveTogether/DST/", ports);
			var serverProcess = _processManager.CreateCmdProcess($"docker run --rm -v {serverDirectory}:/data --name {username}DST -i " +
				$"-p {ports[0]}:{ports[0]}/udp -p {ports[1]}:{ports[1]}/udp " +
				$"-p {ports[2]}:{ports[2]}/udp -p {ports[3]}:{ports[3]}/udp -p {ports[4]}:{ports[4]}/udp dst-server:latest");

			RunningServer server = new RunningServer
			{
				UserId = user.Id,
				ContainerName = username + "DST",
				ServerType = ServerTypes.DstMaster,
				Port = 0
			};

			serverProcess.Start();
			var serverTask = serverProcess.WaitForExitAsync();
			if (!await _processManager.WaitForContainerAsync(username + "DST", true))
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

			return new OkObjectResult(new BaseResponse("Server starting..."));
		}

		private static void SetConfigPorts(string serverDirectory, int[] ports)
		{
			var config = File.ReadAllLines(serverDirectory + "cluster.ini");
			config[31] = config[31].Split("=")[0] + $"= {ports[0]}";
			File.WriteAllLines(serverDirectory + "cluster.ini", config);

			config = File.ReadAllLines(serverDirectory + "Master/server.ini");
			config[1] = config[1].Split("=")[0] + $"= {ports[1]}";
			config[9] = config[9].Split("=")[0] + $"= {ports[2]}";
			File.WriteAllLines(serverDirectory + "Master/server.ini", config);

			config = File.ReadAllLines(serverDirectory + "Caves/server.ini");
			config[1] = config[1].Split("=")[0] + $"= {ports[3]}";
			config[11] = config[11].Split("=")[0] + $"= {ports[4]}";
			File.WriteAllLines(serverDirectory + "Caves/server.ini", config);
		}
	}
}
