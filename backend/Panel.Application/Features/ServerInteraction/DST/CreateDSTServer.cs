using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.DTOs.ServerRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Text.RegularExpressions;

namespace Panel.Application.Features.ServerInteraction.DST
{

	public class CreateDSTServerHandler : IRequestHandler<CreateDSTServerRequest, IActionResult>
	{
		IConfiguration _config;
		IUnitOfWork _unitOfWork;
		IFtpManager _processManager;

		public CreateDSTServerHandler(IConfiguration config, IUnitOfWork unitOfWork, IFtpManager processManager)
		{
			_config = config;
			_unitOfWork = unitOfWork;
			_processManager = processManager;
		}

		public async Task<IActionResult> Handle(CreateDSTServerRequest request, CancellationToken cancellationToken)
		{
			var repository = _unitOfWork.Repository<User>();
			User? accUser = await repository.GetByIdAsync(request.Id);

			if (accUser == null)
				return new NotFoundObjectResult(new BaseResponse(false, "There is no user with such id"));
			if (accUser.DSTServer)
				return new ConflictObjectResult(new BaseResponse(false, "Server is already created"));

			var serversRoot = _config["ServersDirectory"];
			var username = accUser.Email.Replace("@", "");
			var serverDirectory = serversRoot + $"/{username}/DoNotStarveTogether/DST/";

			if (!Directory.Exists(serverDirectory))
				Directory.CreateDirectory(serverDirectory);

			File.WriteAllText(serverDirectory + "cluster_token.txt", request.ServerToken);
			Directory.CreateDirectory(serverDirectory + "Master");
			Directory.CreateDirectory(serverDirectory + "Caves");

			File.Copy($"{serversRoot}/DST templates/master_server.txt", serverDirectory + "Master/server.ini");
			if (request.Worldgen == null)
			{
				File.Copy($"{serversRoot}/DST templates/master_worldgen.txt", serverDirectory + "Master/worldgenoverride.lua");
			}
			else
			{
				var updater = new UpdateFileHandler(_config, _unitOfWork);
				var result = await updater.Handle(new UpdateFileRequest(request.Id, "/DoNotStarveTogether/DST/Master/worldgenoverride.lua", request.Worldgen), CancellationToken.None);
				if (result is not OkResult)
					return new StatusCodeResult(500);
			}

			File.Copy($"{serversRoot}/DST templates/caves_server.txt", serverDirectory + "Caves/server.ini");
			File.Copy($"{serversRoot}/DST templates/caves_worldgen.txt", serverDirectory + "Caves/worldgenoverride.lua");

			var cluster = File.ReadAllLines($"{serversRoot}/DST templates/cluster_template.txt");
			cluster[13] = $"cluster_name = {request.ServerName}";
			cluster[15] = $"cluster_description = {request.ServerDescription}";
			cluster[19] = $"cluster_password = {request.ServerPassword}";
			File.WriteAllLines(serverDirectory + "cluster.ini", cluster);
			Directory.CreateDirectory(serverDirectory + "ugc");
			if (request.Modlist != null)
			{
				string[] setupStrings = new string[request.Modlist.Length];
				for (int i = 0; i < request.Modlist.Length; i++)
					if (Regex.IsMatch(request.Modlist[i], @"/d*"))
						setupStrings[i] = $"ServerModSetup({request.Modlist[i]})";
				File.WriteAllLines(serverDirectory + "ugc/dedicated_server_mods_setup.lua", setupStrings);
			}

			accUser.DSTServer = true;
			await repository.UpdateAsync(accUser);
			await _unitOfWork.Save();
			return new OkObjectResult(new BaseResponse("Created succesfully"));
		}
	}
}
