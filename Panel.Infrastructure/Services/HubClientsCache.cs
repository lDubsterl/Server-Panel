using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Infrastructure.Services
{
	public class ConnectionInfo(string hubConnectionId, Process attachProcess, DateTime lastUsingTime)
	{
		public string HubConnectionId { get; } = hubConnectionId;
		public Process AttachProcess { get; } = attachProcess;
		public DateTime LastUsingTime { get; set; } = lastUsingTime;
		public DataReceivedEventHandler? Handler { get; set; }
	}
	public class HubClientsCache
	{
		public class ProcessInfo(Process attachProcess, DateTime lastUsingTime)
		{
			public Process AttachProcess { get; } = attachProcess;
			public DateTime LastUsingTime { get; set; } = lastUsingTime;
			public DataReceivedEventHandler? Handler { get; set; }
		}

		private readonly ushort _maxConnectionsAmount = 21000;
		private ushort _currentRecordsAmount = 0;
		private uint _id = 1;
		private readonly ConcurrentDictionary<string, uint> _connectionIds = new();
		private readonly ConcurrentDictionary<uint, ProcessInfo> _connections = new();

		public ProcessInfo? GetConnection(uint id)
		{
			_connections.TryGetValue(id, out var info);
			if (info != null)
				_connections[id].LastUsingTime = DateTime.UtcNow;
			return info;
		}
		public uint GetConnectionId(string hubConnectionId) => _connectionIds[hubConnectionId];
		public DateTime GetLastUsingTime(uint id) => _connections[id].LastUsingTime;

		public void SetHandler(uint id, DataReceivedEventHandler handler)
		{
			_connections[id].Handler = handler;
		}
		public void SetConnectionId(string hubConnectionId, uint id) => _connectionIds[hubConnectionId] = id;
		public uint AddConnection(ConnectionInfo connInfo)
		{
			if (_currentRecordsAmount >= _maxConnectionsAmount)
				RemoveIrreleventRecords();
			try
			{
				checked
				{
					_connections[_id] = new ProcessInfo(connInfo.AttachProcess, connInfo.LastUsingTime);
					SetConnectionId(connInfo.HubConnectionId, _id);
				}
			}
			catch (OverflowException)
			{
				_id = 1;
				_connections.Clear();
				_connectionIds.Clear();
				_connections[_id] = new ProcessInfo(connInfo.AttachProcess, connInfo.LastUsingTime);
				SetConnectionId(connInfo.HubConnectionId, _id);
			}

			_currentRecordsAmount++;
			return _id++;
		}

		public bool RemoveHubConnection(string hubId)
		{
			var isRemoved2 = _connectionIds.TryRemove(hubId, out _);
			return isRemoved2;
		}
		public bool RemoveConnection(string hubId)
		{
			var processId = _connectionIds[hubId];
			var isRemoved2 = RemoveHubConnection(hubId);

			bool isRemoved = false;

			if (isRemoved2)
			{
				_connections[processId].AttachProcess.Kill();
				isRemoved = _connections.TryRemove(processId, out _);
			}
			if (isRemoved) _currentRecordsAmount--;
			return isRemoved;
		}
		private void RemoveIrreleventRecords()
		{
			var keys = _connectionIds.Where(connId => _connections[connId.Value].LastUsingTime < DateTime.UtcNow - TimeSpan.FromMinutes(10))
					.Select(conn => conn.Key)
					.ToList();
			foreach (var key in keys)
			{
				RemoveConnection(key);
			}
		}
	}
}
