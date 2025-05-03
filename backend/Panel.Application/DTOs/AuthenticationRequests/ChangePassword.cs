using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.DTOs.AuthenticationRequests
{
	public class ChangePassword: IRequest<IActionResult>
	{
		public int UserId { get; set; }
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string ConfirmPassword { get; set; }
	}
}
