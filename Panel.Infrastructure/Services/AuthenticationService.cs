using Panel.Application.AuthenticationRequests;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Entities;
using Panel.Persistence.Contexts;
using Microsoft.AspNetCore.App;
using Microsoft.EntityFrameworkCore;

namespace Panel.Infrastructure.Services
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly LibraryDbContext _db;
		private readonly ITokenService tokenService;

		public AuthenticationService(LibraryDbContext tasksDbContext, ITokenService tokenService)
		{
			this._db = tasksDbContext;
			this.tokenService = tokenService;
		}

		public async Task<IActionResult> LoginAsync(Login loginRequest)
		{
			var user = _db.Users.SingleOrDefault(user => user.Email == loginRequest.Email);

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
			var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(o => o.UserId == userId);

			if (refreshToken == null)
			{
				return new OkResult();
			}

			_db.RefreshTokens.Remove(refreshToken);

			var saveResponse = await _db.SaveChangesAsync();

			if (saveResponse >= 0)
			{
				return new OkResult();
			}

			return new BadRequestObjectResult(new { Message = "Unable to logout user" });

		}

		public async Task<IActionResult> SignUpAsync(SignUp signupRequest)
		{
			var existingUser = await _db.Users.SingleOrDefaultAsync(user => user.Email == signupRequest.Email);

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

			var user = new User
			{
				Email = signupRequest.Email,
				Password = passwordHash,
				PasswordSalt = Convert.ToBase64String(salt),
				Ts = signupRequest.Ts,
				Role = signupRequest.Role,
			};

			await _db.Users.AddAsync(user);

			var saveResponse = await _db.SaveChangesAsync();

			if (saveResponse >= 0)
			{
				return new OkObjectResult(new { data = user.Email, Message = $"{user.Email} Registered successfully" });
			}

			return new BadRequestObjectResult(new { Message = "Unable to save the user" });
		}
	}
}
