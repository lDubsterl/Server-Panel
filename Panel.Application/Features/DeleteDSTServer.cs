using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features
{
	public class DeleteDSTServer(int id) : ClientId(id), IRequest<IActionResult>
	{

	}

	public class DeleteDSTServerHandler : IRequestHandler<DeleteDSTServer, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;

		public DeleteDSTServerHandler(IUnitOfWork unitOfWork, IConfiguration config)
		{
			_unitOfWork = unitOfWork;
			_config = config;
		}

		public async Task<IActionResult> Handle(DeleteDSTServer request, CancellationToken cancellationToken)
		{
			var repository = _unitOfWork.Repository<UserAccount>();
			var user = await repository.GetByIdAsync(request.Id);
			var dstDedicatedServerRoot = _config["DST_CLI_Directory"];

			if (user == null) return new BadRequestResult();

			Directory.Delete(_config["ServersDirectory"] + user.Email.Replace("@", "") + "/Minecraft/", true);

			File.Delete(@$"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Caves{user.Email}.bat");
			File.Delete(@$"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Master{user.Email}.bat");

			user.DSTServer = false;
			await repository.UpdateAsync(user);
			var processesRepository = _unitOfWork.Repository<RunningServer>();
			await processesRepository.Entities.Where(el => el.UserId == request.Id &&
			(el.ServerType == ConsoleTypes.DstMaster || el.ServerType == ConsoleTypes.DstCaves))
				.ForEachAsync(async el => await processesRepository.DeleteAsync(el));
			await _unitOfWork.Save();

			return new OkResult();

			//dstServerProcesses.Remove(id);
		}
	}
}
