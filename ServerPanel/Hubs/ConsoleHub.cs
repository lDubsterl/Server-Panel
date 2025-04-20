using Docker.DotNet.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Panel.Application.Interfaces.Services;
using Panel.Domain.Common;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Panel.Infrastructure.Hubs
{
	public class ConsoleHub(IUnitOfWork unitOfWork, IOsInteraction osInteractionsService) : Hub
	{
		private static readonly HubClientsCache _connections = new();

		private IUnitOfWork _unitOfWork = unitOfWork;
		private IOsInteraction _processManager = osInteractionsService;
		public override async Task OnConnectedAsync()
		{
			var context = Context.GetHttpContext();

			var containerName = context?.Request.Query["containerName"].ToString();
			var serverType = (ServerTypes)int.Parse(context?.Request.Query["serverType"]);


			var record = await _unitOfWork.Repository<RunningServer>().Entities.Where(s => s.ContainerName == containerName).FirstOrDefaultAsync();
			if (record == null)
			{
				await Clients.Caller.SendAsync("Error", JsonSerializer.Serialize("Server not found"));
				return;
			}

			if (record.ConnectionId == 0)
			{
				var attach = _processManager.CreateCmdProcess("docker attach " + containerName);
				record.ConnectionId = _connections.AddConnection(new ConnectionInfo(Context.ConnectionId, containerName, attach, DateTime.UtcNow));
				attach.Start();
				attach.BeginOutputReadLine();
			}
			else
			{
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
		public async Task ReceiveCommandFromClient(uint connectionId, ServerTypes serverType, string command)
		{
			var attachConnection = _connections.GetConnection(connectionId);
			if (attachConnection == null)
			{
				await Clients.Caller.SendAsync("Error", JsonSerializer.Serialize("Server not found"));
				return;
			}
			if (string.IsNullOrWhiteSpace(command)) return;

			if (serverType == ServerTypes.Minecraft)
			{
				await attachConnection.AttachProcess.StandardInput.WriteLineAsync(command);
				await attachConnection.AttachProcess.StandardInput.FlushAsync();
			}
			else
			{
				var windowNumber = (int)(serverType == ServerTypes.Terraria ? 0 : serverType);
				_processManager.ExecuteCommand($"docker exec {attachConnection.ContainerName} tmux send-keys -t server:{windowNumber} \"{command}\" Enter");
			}

			if (CheckStopCommand(serverType, command))
			{
				if (serverType == ServerTypes.DstCaves) return;
				await _processManager.WaitForContainerAsync(attachConnection.ContainerName, false);
				_processManager.ExecuteCommand($"docker kill {attachConnection.ContainerName}");

				_connections.RemoveConnection(Context.ConnectionId);
				await Clients.Caller.SendAsync("ServerStopped");
			}
		}
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var id = _connections.GetConnectionId(Context.ConnectionId);
			if (id != 0)
				if ((DateTime.UtcNow - _connections.GetLastUsingTime(id)) > TimeSpan.FromMinutes(10))
				{
					var deleted = _connections.RemoveConnection(Context.ConnectionId);
					if (deleted)
					{
						var rep = _unitOfWork.Repository<RunningServer>();
						var record = await rep.Entities.Where(server => server.ConnectionId == id).FirstAsync();
						record.ConnectionId = 0;
						await rep.UpdateAsync(record);
						await _unitOfWork.Save();
					}
				}
				else
					_connections.RemoveHubConnection(Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}
		private static bool CheckStopCommand(ServerTypes serverType, string command)
		{
			switch (serverType)
			{
				case ServerTypes.Minecraft:
					return command == "stop";

				case ServerTypes.Terraria:
					return command == "exit";

				case ServerTypes.DstMaster:
				case ServerTypes.DstCaves:
					{
						var pattern = @"^c_shutdown\s*\(\s*(true|false|[-+]?\d+)?\s*\)\s*;?$";
						return Regex.IsMatch(command.Trim(), pattern);
					}
			}
			return false;
		}
	}
}
