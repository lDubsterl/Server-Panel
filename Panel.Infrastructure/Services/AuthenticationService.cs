﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panel.Application.AuthenticationRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Shared;

namespace Panel.Infrastructure.Services
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly ITokenService tokenService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IFtpManager _manager;

		public AuthenticationService(ITokenService tokenService, IUnitOfWork unitOfWork, IFtpManager manager)
		{
			this.tokenService = tokenService;
			_unitOfWork = unitOfWork;
			_manager = manager;
		}

		public async Task<IActionResult> LoginAsync(Login loginRequest)
		{
			var user = await _unitOfWork.Repository<UserAccount>().Entities.FirstOrDefaultAsync(e => e.Email == loginRequest.Email);
			if (user == null)
			{
				return new BadRequestObjectResult(new { Message = "Email not found" });
			}
			var passwordHash = PasswordBuilder.HashUsingPbkdf2(loginRequest.Password, Convert.FromBase64String(user.PasswordSalt));

			if (user.Password != passwordHash)
			{
				return new BadRequestObjectResult(new { Message = "Invalid password" });
			}

			var token = await Task.Run(() => tokenService.GenerateTokensAsync(user.Id));

			return new OkObjectResult(new { data = token, Message = "Tokens got successfully" });
		}

		public async Task<IActionResult> LogoutAsync(int userId)
		{
			var repository = _unitOfWork.Repository<RefreshToken>();
			var refreshToken = await repository.Entities.FirstOrDefaultAsync(e => e.UserId == userId);

			if (refreshToken == null)
			{
				return new OkResult();
			}

			await repository.DeleteAsync(refreshToken);
			var saveResponse = await _unitOfWork.Save();

			if (saveResponse >= 0)
			{
				return new OkResult();
			}

			return new BadRequestObjectResult(new { Message = "Unable to logout user" });

		}

		public async Task<IActionResult> SignUpAsync(SignUp signupRequest)
		{
			var repository = _unitOfWork.Repository<UserAccount>();
			var existingUser = await repository.Entities.FirstOrDefaultAsync(e => e.Email == signupRequest.Email);

			if (existingUser != null)
			{
				return new BadRequestObjectResult(new { Message = "User with the same email already exists" });
			}

			if (signupRequest.Password != signupRequest.ConfirmPassword)
			{
				return new BadRequestObjectResult(new { Message = "Password and confirm password do not match" });
			}

			if (signupRequest.Password.Length <= 7)
			{
				return new BadRequestObjectResult(new { Message = "Password is weak" });
			}

			var salt = PasswordBuilder.GetSecureSalt();
			var passwordHash = PasswordBuilder.HashUsingPbkdf2(signupRequest.Password, salt);

			var user = new UserAccount
			{
				Email = signupRequest.Email,
				Password = passwordHash,
				PasswordSalt = Convert.ToBase64String(salt),
				FtpPassword = _manager.ManageFTPUser(signupRequest.Email.Replace("@", ""), ManageMode.Create),
				Role = signupRequest.Role,
			};

			await repository.AddAsync(user);
			var saveResponse = await _unitOfWork.Save();
			if (saveResponse >= 0)
			{
				return new OkObjectResult(new { data = user.Email, Message = $"{user.Email} Registered successfully" });
			}

			return new BadRequestObjectResult(new { Message = "Unable to save the user" });
		}
	}
}
