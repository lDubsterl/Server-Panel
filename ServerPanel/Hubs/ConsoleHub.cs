using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Infrastructure.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panel.Infrastructure.Hubs
{
	public class ConsoleHub(IUnitOfWork unitOfWork, IOsInteractionsService osInteractionsService, ILogger<ConsoleHub> logger) : Hub
	{
		private static readonly HubClientsCache _connections = new();

		private IUnitOfWork _unitOfWork = unitOfWork;
		private IOsInteractionsService _processManager = osInteractionsService;
		private ILogger<ConsoleHub> _logger = logger;
		public override async Task OnConnectedAsync()
		{
			var containerName = Context.GetHttpContext()?.Request.Query["containerName"].ToString();

			var record = await _unitOfWork.Repository<RunningServer>().Entities.Where(s => s.ContainerName == containerName).FirstOrDefaultAsync();
			if (record == null)
			{
				await Clients.Caller.SendAsync("Error", JsonSerializer.Serialize("Server not found"));
				return;
			}

			Process attach;

			if (record.ConnectionId == 0)
			{
				attach = _processManager.CreateCmdProcess("docker attach " + containerName);
				record.ConnectionId = _connections.AddConnection(new ConnectionInfo(Context.ConnectionId, containerName, attach, DateTime.UtcNow));
				attach.ErrorDataReceived += (sender, args) =>
				{
					if (!string.IsNullOrEmpty(args.Data))
					{
						_logger.LogInformation("[STDERR] " + args.Data);
					}
				};
				attach.Start();
				attach.BeginErrorReadLine();
				attach.BeginOutputReadLine();
			}
			else
			{
				attach = _connections.GetConnection(record.ConnectionId).AttachProcess;
				_connections.SetConnectionId(Context.ConnectionId, record.ConnectionId);
			}

			SendLogsToClient(record.ConnectionId, Clients.Caller);

			var successResponse = JsonSerializer.Serialize(new
			{
				connectionId = record.ConnectionId,
				message = $"Connected successfully to {record.ContainerName}"
			});
			await Clients.Caller.SendAsync("InfoReceive", successResponse);
			await base.OnConnectedAsync();

			await _unitOfWork.Repository<RunningServer>().UpdateAsync(record);
			await _unitOfWork.Save();
		}

		public void SendLogsToClient(uint id, IClientProxy client)
		{
			var conn = _connections.GetConnection(id);

			var _outputDataReceivedHandler = new DataReceivedEventHandler(async (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					await client.SendAsync("Receive", e.Data);
				}
			});

			if (conn.Handler != null)
				conn.AttachProcess.OutputDataReceived -= conn.Handler;
			conn.AttachProcess.OutputDataReceived += _outputDataReceivedHandler;
			_connections.SetHandler(id, _outputDataReceivedHandler);
		}

		public async Task ReceiveCommandFromClient(uint connectionId, string command)
		{
			var attachConnection = _connections.GetConnection(connectionId);
			if (attachConnection == null)
			{
				await Clients.Caller.SendAsync("Error", JsonSerializer.Serialize("Server not found"));
				return;
			}
			if (string.IsNullOrWhiteSpace(command)) return;
			await attachConnection.AttachProcess.StandardInput.WriteLineAsync(command);
			await attachConnection.AttachProcess.StandardInput.FlushAsync();
			if (command == "stop")
			{
				await WaitForContainerStopAsync(attachConnection.ContainerName);

				var rep = _unitOfWork.Repository<RunningServer>();
				var record = await rep.Entities.Where(server => server.ContainerName == attachConnection.ContainerName).FirstAsync();
				record.ConnectionId = 0;

				_connections.RemoveConnection(Context.ConnectionId);
				await rep.UpdateAsync(record);
				await _unitOfWork.Save();
				await Clients.Caller.SendAsync("ServerStopped");
			}
		}
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var id = _connections.GetConnectionId(Context.ConnectionId);
			if (id != 0)
				if ((DateTime.UtcNow - _connections.GetLastUsingTime(id)) > TimeSpan.FromMinutes(10))
				{
					var exists = _connections.RemoveConnection(Context.ConnectionId);
					if (exists)
					{
						var rep = _unitOfWork.Repository<RunningServer>();
						var record = await rep.Entities.Where(server => server.ConnectionId == id).FirstAsync();
						record.ConnectionId = 0;
						await rep.UpdateAsync(record);
						await _unitOfWork.Save();
					}
				}
				else
				{
					_connections.RemoveHubConnection(Context.ConnectionId);
				}
			await base.OnDisconnectedAsync(exception);
		}
		private async Task WaitForContainerStopAsync(string containerName, int timeoutSeconds = 10)
		{
			var stopwatch = Stopwatch.StartNew();
			while (stopwatch.Elapsed < TimeSpan.FromSeconds(timeoutSeconds))
			{
				var result = _processManager.ExecuteCommand($"docker inspect -f '{{{{.State.Running}}}}' {containerName}");
				if (result.Trim() == "false")
					return;
				await Task.Delay(500); // Подождем полсекунды перед повторной проверкой
			}
			_processManager.ExecuteCommand($"docker kill {containerName}");
			return;
		}

	}
}
