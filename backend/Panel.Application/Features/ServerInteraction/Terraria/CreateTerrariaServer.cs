using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction.Terraria
{
	public class CreateTerrariaServerHandler : IRequestHandler<CreateTerrariaServerRequest, IActionResult>
	{
		IConfiguration _config;
		IUnitOfWork _unitOfWork;
		IFtpManager _processManager;

		public CreateTerrariaServerHandler(IConfiguration config, IUnitOfWork unitOfWork, IFtpManager processManager)
		{
			_config = config;
			_unitOfWork = unitOfWork;
			_processManager = processManager;
		}

		public async Task<IActionResult> Handle(CreateTerrariaServerRequest request, CancellationToken cancellationToken)
		{
			var repository = _unitOfWork.Repository<User>();
			User? accUser = await repository.GetByIdAsync(request.UserId);

			if (accUser == null)
				return new NotFoundObjectResult(new BaseResponse(false, "There is no user with such id"));
			if (accUser.TerrariaServer)
				return new ConflictObjectResult(new BaseResponse(false, "Server is already created"));

			var serversRoot = _config["ServersDirectory"];
			var username = accUser.Email.Replace("@", "");
			var serverDirectory = serversRoot + $"/{username}/Terraria/";

			if (!Directory.Exists(serverDirectory))
				Directory.CreateDirectory(serverDirectory);

			if (request.Config == null)
			{
				File.Copy($"{serversRoot}/Terraria/tModLoader/serverconfig.txt", serverDirectory);
			}
			else
			{
				var updater = new UpdateFileHandler(_config, _unitOfWork);
				var result = await updater.Handle(new UpdateFileRequest(request.UserId, "/Terraria/serverconfig.txt", request.Config), CancellationToken.None);
				if (result is not OkResult)
					return new StatusCodeResult(500);
			}

			if (request.Modlist != null)
			{
				Directory.CreateDirectory(serverDirectory + "Mods");
				using var modlistFile = new StreamWriter(serverDirectory + "Mods/install.txt");
				foreach (var mod in request.Modlist)
					modlistFile.WriteLine(mod);
			}

			accUser.TerrariaServer = true;
			await repository.UpdateAsync(accUser);
			await _unitOfWork.Save();
			return new OkObjectResult(new BaseResponse("Created succesfully"));
		}
	}
}
