using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ServerPanel.Controllers
{
	public class BaseController : ControllerBase
	{
		protected int UserId => int.Parse(FindClaim(ClaimTypes.NameIdentifier));
		protected string Role => FindClaim(ClaimTypes.Role);
		string FindClaim(string claimName)
		{
			var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
			var claim = claimsIdentity?.FindFirst(claimName);
			if (claim == null) return null;
			return claim.Value;
		}
	}
}
