using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.AuthenticationRequests
{
    public class SignUp : IRequest<IActionResult>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
    }
}
