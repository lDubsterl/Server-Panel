using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Npgsql;
using ServerPanel.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using TokenApp;
using BC = BCrypt.Net.BCrypt;

namespace ServerPanel.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : Controller
	{
		string connectionString = "Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;";
		
		[HttpPost("/registration")]
		public JsonResult RegisterUser(string email, string password)
		{
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
				var acc = db.Query<UserAccount>("select * from \"Site accounts\" where Email = @email", new { email }).FirstOrDefault();
				if (acc is not null)
					return Json("User already registered");
				string hashedPassword = BC.HashPassword(password);
				db.Execute("insert into \"Site accounts\"(Email, Password) values (@email, @hashedPassword)", new { email, hashedPassword });
			}
			return Json("Registered succesfully");
		}

		[HttpPost("/authorization")]
		public IActionResult AuthorizeUser(JObject jsonData)
		{
			var username = (string)jsonData.GetValue("email");
			var password = (string)jsonData.GetValue("password");
			var identity = CheckUser(username, password);
			if (identity == null)
			{
				return BadRequest(new { errorText = "Invalid username or password." });
			}

			var now = DateTime.UtcNow;
			// создаем JWT-токен
			var jwt = new JwtSecurityToken(
					issuer: AuthOptions.ISSUER,
					audience: AuthOptions.AUDIENCE,
					notBefore: now,
					claims: identity.Claims,
					expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
					signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

			var response = new
			{
				access_token = encodedJwt,
				username = identity.Name,
				id = identity.Claims.ElementAt(1).Value
			};

			return Json(response);
		}

		private ClaimsIdentity CheckUser(string username, string password)
		{
			UserAccount acc;
			using (IDbConnection db = new NpgsqlConnection(connectionString))
			{
			
				acc = db.Query<UserAccount>("select * from \"Site accounts\" where Email = @username", new { username }).FirstOrDefault();
			}
			if (acc != null)
			{
				string passwordHash = acc.Password;
				if (!BC.Verify(password, passwordHash))
					return null;
				var claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, acc.Email),
					new Claim(ClaimsIdentity.DefaultNameClaimType, acc.Id.ToString())
				};
				ClaimsIdentity claimsIdentity =
				new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType);
				return claimsIdentity;
			}

			// если пользователя не найдено
			return null;
		}
	}
}
