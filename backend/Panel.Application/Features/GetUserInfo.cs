using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;

namespace Panel.Application.Features
{
	public class GetUserInfoRequest(int userId) : IRequest<IActionResult>
	{
		public int UserId { get; set; } = userId;
	}
	public class GetUserInfoHandler : IRequestHandler<GetUserInfoRequest, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IOsInteraction _cmd;

		public GetUserInfoHandler(IUnitOfWork unitOfWork, IOsInteraction cmd)
		{
			_unitOfWork = unitOfWork;
			_cmd = cmd;
		}

		public async Task<IActionResult> Handle(GetUserInfoRequest request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
			if (user == null)
				return new NotFoundResult();
			return new OkObjectResult(new
			{
				user.Email,
				FtpCreds = _cmd.ExecuteCommand("curl ifconfig.me") + "@" + user.Email.Replace("@", "") + ":" + user.FtpPassword,
				RegistrationDate = user.Ts
			});
		}
	}
}
