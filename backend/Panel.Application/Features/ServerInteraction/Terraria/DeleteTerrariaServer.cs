using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction.Terraria
{
	public class DeleteTerrariaServer(int id) : IRequest<IActionResult>
	{
		public int Id { get; } = id;
	}

	public class DeleteMinecraftServerHandler : IRequestHandler<DeleteTerrariaServer, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;

		public DeleteMinecraftServerHandler(IUnitOfWork unitOfWork, IConfiguration config)
		{
			_unitOfWork = unitOfWork;
			_config = config;
		}

		public async Task<IActionResult> Handle(DeleteTerrariaServer request, CancellationToken cancellationToken)
		{
			var userRepository = _unitOfWork.Repository<User>();
			var user = await userRepository.GetByIdAsync(request.Id);

			if (user == null) return new BadRequestObjectResult(new BaseResponse(false, "User not found"));

			Directory.Delete(_config["ServersDirectory"] + "/" + user.Email.Replace("@", "") + "/Terraria/", true);

			user.TerrariaServer = false;

			var processesRepository = _unitOfWork.Repository<RunningServer>();
			var server = await processesRepository.Entities.FirstOrDefaultAsync(el => el.UserId == request.Id &&
			el.ServerType == ServerTypes.Terraria);
			
			await userRepository.UpdateAsync(user);
			if (server != null)
				await processesRepository.DeleteAsync(server);
			await _unitOfWork.Save();

			return new OkObjectResult(new BaseResponse("Server deleted successfully"));
		}
	}
}
