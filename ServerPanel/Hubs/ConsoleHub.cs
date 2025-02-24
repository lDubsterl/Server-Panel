using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Panel.Infrastructure.Hubs
{
	public class ConsoleHub : Hub
	{
		private static string _baseUrl = "ws://localhost:2375/containers/";
		private static string _baseArgs = "/attach/ws?stream=1&stdin=1&stdout=1";

		private IUnitOfWork _unitOfWork;
		private IDistributedCache _cache;

		private bool _isLogsNeeded = true;
		public ConsoleHub(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
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
			if (record.ConsoleConnection == null)
			{
				client = new ClientWebSocket();
				record.ConsoleConnection = client;
			}
			else
				client = record.ConsoleConnection;

			_isLogsNeeded = true;
			if (client.State != WebSocketState.Open)
			{
				await client.ConnectAsync(url, CancellationToken.None);
				_ = ReceiveLogs(client, Clients.Caller);
			}

			var response = JsonSerializer.Serialize(new
			{
				serverId = record.Id,
				message = $"Connected successfully to {record.ContainerName}"
			});
			await Clients.Caller.SendAsync("Connected", response);
			await base.OnConnectedAsync();

			await _unitOfWork.Repository<RunningServer>().UpdateAsync(record);
			await _cache.SetStringAsync($"{record.GetType().Name}:{record.Id}", JsonSerializer.Serialize(record));
			await _unitOfWork.Save();
		}
		public async Task SendCommand(int serverId, string command)
		{
			var record = await _unitOfWork.Repository<RunningServer>().GetByIdAsync(serverId);
			if (record == null)
			{
				await Clients.Caller.SendAsync("ErrorSend", "Server not found");
				return;
			}
			var cmd = Encoding.UTF8.GetBytes(command + "\n");
			await record.ConsoleConnection.SendAsync(cmd, WebSocketMessageType.Text, true, CancellationToken.None);
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
