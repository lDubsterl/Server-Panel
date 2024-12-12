using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
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
			var repository = _unitOfWork.Repository<UserAccount>("Site Accounts");
			var user = await repository.GetByIdAsync(request.Id);
			var dstDedicatedServerRoot = _config["DST_CLI_Directory"];

			Directory.Delete(_config["DSTServerDirectory"] + user.Email, true);
			File.Delete(@$"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Caves{user.Email}.bat");
			File.Delete(@$"{dstDedicatedServerRoot}Don't Starve Together Dedicated Server\bin\Server_Master{user.Email}.bat");

			user.DSTServer = false;
			await repository.UpdateAsync(user);
			return new OkResult();

			//dstServerProcesses.Remove(id);
		}
	}
}
