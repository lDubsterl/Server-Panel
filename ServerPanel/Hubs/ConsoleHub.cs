using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using Panel.Infrastructure.Services;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Panel.Infrastructure.Hubs
{
	public class ConsoleHub(IUnitOfWork unitOfWork) : Hub
	{
		private const string _baseUrl = "ws://host.docker.internal:2375/containers/";
		private const string _baseArgs = "/attach/ws?stream=1&stdin=1&stdout=1";
		private static readonly WebSocketHubCache _connections = new();

		private IUnitOfWork _unitOfWork = unitOfWork;

		private bool _isLogsNeeded = true;

		public override async Task OnConnectedAsync()
		{
			var containerName = Context.GetHttpContext()?.Request.Query["containerName"].ToString();

			var record = await _unitOfWork.Repository<RunningServer>().Entities.Where(s => s.ContainerName == containerName).FirstOrDefaultAsync();
			if (record == null)
			{
				await Clients.Caller.SendAsync("ErrorSend", "Server not found");
				return;
			}

			ClientWebSocket client;
			var url = new Uri(_baseUrl + containerName + _baseArgs);

			if (record.ConnectionId == 0)
			{
				client = new ClientWebSocket();
				record.ConnectionId = _connections.AddConnection(new WebSocketHubCache.ConnectionInfo(client, DateTime.UtcNow));
			}
			else
				client = _connections.GetConnection(record.ConnectionId);

			if (client.State != WebSocketState.Open)
			{
				await client.ConnectAsync(url, CancellationToken.None);
				_ = ReceiveLogs(client, Clients.Caller);
			}

			var response = JsonSerializer.Serialize(new
			{
				connectionId = record.ConnectionId,
				message = $"Connected successfully to {record.ContainerName}"
			});
			await Clients.Caller.SendAsync("Connected", response);
			await base.OnConnectedAsync();

			await _unitOfWork.Repository<RunningServer>().UpdateAsync(record);
			await _unitOfWork.Save();
		}
		public async Task SendCommand(uint connectionId, string command)
		{
			var socket = _connections.GetConnection(connectionId);
			if (socket == null)
			{
				await Clients.Caller.SendAsync("ErrorSend", "Server not found");
				return;
			}
			var cmd = Encoding.UTF8.GetBytes(command + "\n");
			await socket.SendAsync(cmd, WebSocketMessageType.Text, true, CancellationToken.None);
		}
		public async Task ReceiveLogs(ClientWebSocket clientSocket, IClientProxy client)
		{
			var buffer = new byte[1024];
			while (clientSocket.State == WebSocketState.Open && _isLogsNeeded)
			{
				var result = await clientSocket.ReceiveAsync(buffer, CancellationToken.None);
				var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

				await client.SendAsync("Receive", message, CancellationToken.None);
			}
			if (clientSocket.State != WebSocketState.Open)
				await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
		}
		public override Task OnDisconnectedAsync(Exception exception)
		{
			_isLogsNeeded = false;
			return base.OnDisconnectedAsync(exception);
		}

	}
}
