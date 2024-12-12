using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features
{
	public class DeleteMinecraftServer(int id): ClientId(id), IRequest<IActionResult>
	{

	}

	public class DeleteMinecraftServerHandler: IRequestHandler<DeleteMinecraftServer, IActionResult>
	{
		private IUnitOfWork _unitOfWork;
		private IConfiguration _config;

		public DeleteMinecraftServerHandler(IUnitOfWork unitOfWork, IConfiguration config)
		{
			_unitOfWork = unitOfWork;
			_config = config;
		}

		public async Task<IActionResult> Handle(DeleteMinecraftServer request, CancellationToken cancellationToken)
		{
			var repository = _unitOfWork.Repository<UserAccount>("Site Accounts");
			var user = await repository.GetByIdAsync(request.Id);

			Directory.Delete(_config["ServersDirectory"] + user.Email, true);

			user.MinecraftServer = false;
			await repository.UpdateAsync(user);
			return new OkResult();
		}
	}
}
