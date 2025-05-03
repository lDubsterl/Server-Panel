using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction.DST
{
	public class DeleteDSTServer(int id) : IRequest<IActionResult>
	{
		public int Id { get; } = id;
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
			var repository = _unitOfWork.Repository<User>();
			var user = await repository.GetByIdAsync(request.Id);

			if (user == null) return new BadRequestObjectResult(new BaseResponse(false, "User not found"));

			Directory.Delete(_config["ServersDirectory"] + "/" + user.Email.Replace("@", "") + "/DoNotStarveTogether/", true);

			user.DSTServer = false;

			var processesRepository = _unitOfWork.Repository<RunningServer>();
			var entity = await processesRepository.Entities.FirstOrDefaultAsync(el => el.UserId == request.Id &&
			el.ServerType == ServerTypes.DstMaster);

			await repository.UpdateAsync(user);
			if (entity is not null) 
				await processesRepository.DeleteAsync(entity);
			await _unitOfWork.Save();

			return new OkObjectResult(new BaseResponse("Server deleted successfully"));

		}
	}
}
