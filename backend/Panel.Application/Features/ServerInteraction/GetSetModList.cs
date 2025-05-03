using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Text.RegularExpressions;

namespace Panel.Application.Features.ServerInteraction
{
	public class GetSetModList : IRequest<IActionResult>
	{
		public int UserId { get; set; }
		public ServerTypes ServerType { get; set; }
		public ModStatus[]? Modlist { get; set; }
	}
	public class ModStatus
	{
		public string ModName { get; set; }
		public bool Status { get; set; }
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
			return request.ServerType switch
			{
				ServerTypes.DstMaster => HandleDstMods(username, request.Modlist),
				ServerTypes.Terraria => HandleTerrariaMods(username, request.Modlist),
				_ => new NotFoundResult()
			};
		}

		private IActionResult HandleDstMods(string username, ModStatus[]? modlist)
		{
			var modsConfigFile = $"{_config["ServersDirectory"]}/{username}/DoNotStarveTogether/DST/ugc/dedicated_server_mods_setup.lua";
			var modNumbersList = File.ReadAllLines(modsConfigFile);

			string[] modList = new string[modNumbersList.Length];
			var modsPath = $"{_config["ServersDirectory"]}/{username}/DoNotStarveTogether/DST/ugc/content/322330/";

			if (!Directory.Exists(modsPath))
				return new NotFoundObjectResult(new BaseResponse(false, "Directory isn't created yet"));

			if (modlist == null)
			{
				int i = 0;
				foreach (var mod in modNumbersList)
				{
					string pattern = @"\((\d+)\)";
					Match match = Regex.Match(mod, pattern);

					string extractedNumber = match.Groups[1].Value;
					var modName = File.ReadLines($"{modsPath}{extractedNumber}/modinfo.lua")
						.Take(4)
						.FirstOrDefault(name => name.Contains("name"), "=undefined")
						.Split("=")[1];
					modList[i++] = modName;
				}
				return new OkObjectResult(modList);
			}
			else
			{
				string[] setupStrings = new string[modlist.Length];
				var installedModsList = File.ReadLines(modsConfigFile);
				using var mods = new StreamWriter(modsConfigFile, false);
				for (int i = 0; i < modlist.Length; i++)
					if (Regex.IsMatch(modlist[i].ModName, @"\d+") && !installedModsList.Contains(modlist[i].ModName) && modlist[i].Status)
						mods.WriteLine($"ServerModSetup({modlist[i].ModName})");
				return new OkResult();
			}
		}

		private IActionResult HandleTerrariaMods(string username, ModStatus[]? modlist)
		{
			var modsDirectory = $"{_config["ServersDirectory"]}/{username}/Terraria/Mods/";
			var installedMods = modsDirectory + "install.txt";
			var enabledMods = modsDirectory + "enabled.json";

			if (modlist == null)
			{
				var startLines = File.ReadLines(enabledMods).Take(3).ToArray();
				var modNumbersList = File.ReadAllLines(installedMods);
				string?[] modList = new string[modNumbersList.Length];
				var modsPath = $"{_config["ServersDirectory"]}/{username}/Terraria/steamapps/workshop/content/1281930/";

				if (!Directory.Exists(modsPath))
					return new NotFoundObjectResult(new BaseResponse(false, "Directory isn't created yet"));

				for (int i = 0; i < modNumbersList.Length; i++)
				{
					string modFile = "";
					if (Directory.Exists(modsPath + modNumbersList[i]))
						modFile = Directory.EnumerateFiles(modsPath + modNumbersList[i], "*.tmod", SearchOption.AllDirectories)
									.Distinct()
									.FirstOrDefault("");

					if (string.IsNullOrEmpty(modFile))
						modList[i] = modNumbersList[i];
					else
						modList[i] = Path.GetFileNameWithoutExtension(modFile);
				}
				if (startLines[0] == "[]" || startLines.Length < 3)
				{
					using var enabledWriter = new StreamWriter(enabledMods, false);
					enabledWriter.WriteLine("[");
					for (int i = 0; i < modNumbersList.Length; i++)
					{
						if (!Regex.IsMatch(modList[i]!, "\\d+"))
							if (i < startLines.Length - 1)
								enabledWriter.WriteLine($"  \"{modList[i]}\",");
							else
								enabledWriter.WriteLine($"  \"{modList[i]}\"");
					}
					enabledWriter.Write("]");
					return new OkObjectResult(modList.Select(item => new ModStatus { ModName = item!, Status = !Regex.IsMatch(item!, "/d*") }));
				}
				else
				{
					using var enabledReader = new StreamReader(enabledMods);
					enabledReader.ReadLine();

					var mods = new List<ModStatus>();
					string enabledMod = "";
					foreach (var installedMod in modList)
					{
						var status = false;
						enabledMod = enabledReader.ReadLine()!;
						if (enabledMod != "]")
							status = true;
						mods.Add(new ModStatus { ModName = installedMod!, Status = status });
					}
					return new OkObjectResult(mods.ToArray());
				}
			}
			else
			{
				using var enabledWriter = new StreamWriter(enabledMods, false);
				enabledWriter.WriteLine("[");
				var newMods = new List<string>();
				var installedModsList = File.ReadLines(installedMods);
				for (int i = 0; i < modlist.Length; i++)
				{
					if (Regex.IsMatch(modlist[i].ModName, "\\d+") && !installedModsList.Contains(modlist[i].ModName))
						newMods.Add(modlist[i].ModName);
					else if (modlist[i].Status)
						if (i < modlist.Length - 1)
							enabledWriter.WriteLine($"  \"{modlist[i].ModName}\",");
						else
							enabledWriter.WriteLine($"  \"{modlist[i].ModName}\"");
				}
				enabledWriter.Write("]");
				if (newMods.Count > 0)
					File.AppendAllLines(installedMods, ["\n", .. newMods]);
				return new OkResult();
			}
		}
	}
}
