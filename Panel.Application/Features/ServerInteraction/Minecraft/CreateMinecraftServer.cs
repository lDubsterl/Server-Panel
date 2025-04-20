using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Panel.Application.DTOs.ServerRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction.Minecraft
{

	public class CreateMinecraftServerHandler : IRequestHandler<CreateMinecraftServerRequest, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;
		private IOsInteraction _processManager;

		public CreateMinecraftServerHandler(IUnitOfWork unitOfWork, IConfiguration config, IOsInteraction processManager, ILogger<CreateMinecraftServerHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_config = config;
			_processManager = processManager;
		}

		public async Task<IActionResult> Handle(CreateMinecraftServerRequest request, CancellationToken cancellationToken)
		{
			var _serversRoot = _config["ServersDirectory"];
			var repository = _unitOfWork.Repository<User>();

			var client = await repository.GetByIdAsync(request.Id);

			if (client == null)
				return new NotFoundObjectResult(new BaseResponse(false, "There is no user with such id"));

			if (client.MinecraftServerExecutable != "")
				return new ConflictObjectResult(new BaseResponse(false, "Server is already created"));

			var serverDirectory = $"{_serversRoot}/{client.Email.Replace("@", "")}/Minecraft/";

			if (!Directory.Exists(serverDirectory))
				Directory.CreateDirectory(serverDirectory);

			if (request.ServerExecutableName.Contains("forge") || request.ServerExecutableName.Contains("fabric"))
			{
				await _processManager.ExecuteCommandAsync($"java -jar ../../{request.ServerExecutableName} --installServer", serverDirectory);
				client.MinecraftServerExecutable = $"\"ash {Directory.GetFiles(serverDirectory, "*.sh")
					.First(fname => fname.Contains("start") || fname.Contains("run"))}\"";
			}
			else
			{
				File.Copy($"{_serversRoot}/{request.ServerExecutableName}", serverDirectory + "server.jar");
				client.MinecraftServerExecutable = "'java -jar server.jar nogui'";
			}

			var eula = "eula=true";
			File.WriteAllText(serverDirectory + "eula.txt", eula);

			await repository.UpdateAsync(client);
			await _unitOfWork.Save();
			return new OkObjectResult(new BaseResponse("Created succesfully"));
		}
	}
}
