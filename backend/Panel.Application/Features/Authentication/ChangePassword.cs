using MediatR;
using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.Features.Authentication
{
	public class ChangePasswordHandler : IRequestHandler<ChangePassword, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IAuthenticationService _authenticationService;

		public ChangePasswordHandler(IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
		{
			_unitOfWork = unitOfWork;
			_authenticationService = authenticationService;
		}

		public async Task<IActionResult> Handle(ChangePassword request, CancellationToken cancellationToken)
		{
			if (request.NewPassword != request.ConfirmPassword)
				return new BadRequestObjectResult("Введённые новый и повторный пароли не совпадают");

			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
			if (user == null)
				return new NotFoundObjectResult("Пользователь не найден");

			user = _authenticationService.ChangePassword(request.CurrentPassword, user, request.NewPassword);
			if (user == null)
				return new BadRequestObjectResult("Неверный пароль");

			await _unitOfWork.Repository<User>().UpdateAsync(user);
			await _unitOfWork.Save();
			return new OkResult();
		}
	}
}
