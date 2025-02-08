using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Panel.Application.Common;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Diagnostics;

namespace Panel.Application.Features
{
	public class StartDSTServer(int id): ClientId(id), IRequest<IActionResult>
	{
	}

	public class StartDSTServerHandler : IRequestHandler<StartDSTServer, IActionResult>
	{
		IUnitOfWork _unitOfWork;
		IProcessManager _processManager;
		IConfiguration _configuration;
		IConsoleHub _hub;

		public StartDSTServerHandler(IUnitOfWork unitOfWork, IProcessManager processManager, IConfiguration configuration, IConsoleHub consoleHub)
		{
			_unitOfWork = unitOfWork;
			_processManager = processManager;
			_configuration = configuration;
			_hub = consoleHub;
		}

		public async Task<IActionResult> Handle(StartDSTServer request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Repository<UserAccount>().GetByIdAsync(request.Id);

			var serverDirectory = $"{_configuration["DST_CLI_Directory"]}Don't Starve Together Dedicated Server\\bin\\";
			if (user == null) return new BadRequestResult();

			Process masterProcess = _processManager.CreateCmdProcess($"{serverDirectory}Server_Master{user.Email}.bat");
			Process cavesProcess = _processManager.CreateCmdProcess($"{serverDirectory}Server_Caves{user.Email}.bat");


			masterProcess.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(args.Data);
					await _hub.Send(ConsoleTypes.DstMaster.ToString(), args.Data + "\n", request.Id);
				}
			};

			var masterServer = new RunningServer
			{
				UserId = user.Id,
				ServerProcessId = masterProcess.Id,
				ServerType = ConsoleTypes.DstMaster
			};

			masterProcess.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(ConsoleTypes.DstMaster.ToString(), "ERROR: " + args.Data);
					//await _hub.Send(ConsoleTypes.DstMaster.ToString(), "ERROR: " + args.Data + "\n", request.Id);
				}
			};

			cavesProcess.OutputDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(args.Data);
					await _hub.Send(ConsoleTypes.DstCaves.ToString(), args.Data + "\n", request.Id);
				}
			};

			var cavesServer = new RunningServer
			{
				UserId = user.Id,
				ServerProcessId = cavesProcess.Id,
				ServerType = ConsoleTypes.DstCaves,
			};

			cavesProcess.ErrorDataReceived += async (sender, args) =>
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Console.WriteLine(ConsoleTypes.DstCaves.ToString(), "ERROR: " + args.Data);
					//await _hub.Send(ConsoleTypes.DstCaves.ToString(), "ERROR: " + args.Data + "\n", request.Id);
				}
			};

			masterProcess.Start();
			cavesProcess.Start();
			masterProcess.BeginErrorReadLine();
			masterProcess.BeginOutputReadLine();
			cavesProcess.BeginErrorReadLine();
			cavesProcess.BeginOutputReadLine();

			await _unitOfWork.Repository<RunningServer>().AddAsync(masterServer);
			await _unitOfWork.Repository<RunningServer>().AddAsync(cavesServer);
			await _unitOfWork.Save();

			_ = Task.Run(async () =>
			{
				await masterProcess.WaitForExitAsync();
				await _unitOfWork.Repository<RunningServer>().DeleteAsync(masterServer);
				await cavesProcess.WaitForExitAsync();
				await _unitOfWork.Repository<RunningServer>().DeleteAsync(cavesServer);
				await _unitOfWork.Save();
			});

			return new OkObjectResult("Server starting...");
		}
	}
}
