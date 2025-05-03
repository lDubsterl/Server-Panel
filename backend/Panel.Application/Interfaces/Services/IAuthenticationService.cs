using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.AuthenticationRequests;
using Panel.Domain.Models;

namespace Panel.Application.Interfaces.Services
{
    public interface IAuthenticationService
	{
		Task<IActionResult> LoginAsync(Login request);
		Task<IActionResult> SignUpAsync(SignUp request);
		Task<IActionResult> LogoutAsync(int userId);
		User? ChangePassword(string password, User user, string newPassword);
	}
}
