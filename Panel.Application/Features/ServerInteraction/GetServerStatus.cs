using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features.ServerInteraction
{
	public class GetServerStatusRequest : IRequest<IActionResult>
	{
		public int UserId { get; set; }

		public GetServerStatusRequest(int id)
		{
			UserId = id;
		}
	}

	public class GetServerStatusHandler : IRequestHandler<GetServerStatusRequest, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IOsInteractionsService _processManager;

		public GetServerStatusHandler(IUnitOfWork unitOfWork, IOsInteractionsService proceeManager)
		{
			_unitOfWork = unitOfWork;
			_processManager = proceeManager;
		}

		public async Task<IActionResult> Handle(GetServerStatusRequest request, CancellationToken cancellationToken)
		{
			var server = await _unitOfWork.Repository<RunningServer>().Entities.Where(s => s.UserId == request.UserId).FirstOrDefaultAsync();

			if (server == null)
				return new OkObjectResult(new { isStarted = false });
			return new OkObjectResult(new
			{
				isStarted = true,
				address = _processManager.ExecuteCommand("curl ifconfig.me") + ":" + server.Port,
				containerName = server.ContainerName
			});
		}
	}
}
