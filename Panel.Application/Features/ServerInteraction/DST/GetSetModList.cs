using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Text.RegularExpressions;

namespace Panel.Application.Features.ServerInteraction.DST
{
	public class GetSetModList : IRequest<IActionResult>
	{
		public int UserId { get; set; }
		public string[]? Modlist { get; set; }
	}
	public class GetSetModListHandler : IRequestHandler<GetSetModList, IActionResult>
	{
		IConfiguration _config;
		IUnitOfWork _unitOfWork;

		public GetSetModListHandler(IConfiguration config, IUnitOfWork unitOfWork)
		{
			_config = config;
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Handle(GetSetModList request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);

			if (user is null)
				return new NotFoundObjectResult(new BaseResponse(false, "User not found"));

			var username = user.Email.Replace("@", "");
			var modsConfigFile = $"{_config["ServersDirectory"]}/{username}/DoNotStarveTogether/DST/mods/dedicated_server_mods_setup.lua";
			var modNumbersList = File.ReadAllLines(modsConfigFile);

			string[] modList = new string[modNumbersList.Length];
			var modsPath = $"{_config["ServersDirectory"]}/{username}/DoNotStarveTogether/DST/ugc/content/322330/";
			if (request.Modlist == null)
			{
				int i = 0;
				foreach (var mod in modNumbersList)
				{
					string pattern = @"\((\d+)\)";
					Match match = Regex.Match(mod, pattern);

					string extractedNumber = match.Groups[1].Value;
					var modName = File.ReadLines($"{modsPath}{extractedNumber}/modinfo.lua")
						.Take(4)
						.FirstOrDefault(name => name.Contains("name"), "=")
						.Split("=")[1];
					modList[i++] = modName;
				}
				return new OkObjectResult(new { data = modList });
			}
			else
			{
				string[] setupStrings = new string[request.Modlist.Length];
				for (int i = 0; i < request.Modlist.Length; i++)
					if (Regex.IsMatch(request.Modlist[i], @"/d*"))
						setupStrings[i] = $"ServerModSetup({request.Modlist[i]})";
				File.WriteAllLines(modsConfigFile, setupStrings);
				return new OkResult();
			}
		}
	}
}
