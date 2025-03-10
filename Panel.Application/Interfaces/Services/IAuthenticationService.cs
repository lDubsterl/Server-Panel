﻿using Microsoft.AspNetCore.Mvc;
using Panel.Application.DTOs.AuthenticationRequests;

namespace Panel.Application.Interfaces.Services
{
    public interface IAuthenticationService
	{
		Task<IActionResult> LoginAsync(Login request);
		Task<IActionResult> SignUpAsync(SignUp request);
		Task<IActionResult> LogoutAsync(int userId);
	}
}
