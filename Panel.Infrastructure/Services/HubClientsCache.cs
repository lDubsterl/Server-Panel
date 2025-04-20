using Panel.Domain.Common;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Panel.Infrastructure.Services
{
	public class ConnectionInfo(string hubConnectionId, string containerName, Process attachProcess, DateTime lastUsingTime)
	{
		public string HubConnectionId { get; } = hubConnectionId;
		public Process AttachProcess { get; } = attachProcess;
		public DateTime LastUsingTime { get; set; } = lastUsingTime;
		public string ContainerName { get; } = containerName;
	}
	public class HubClientsCache
	{
		public class ProcessInfo(string containerName, Process attachProcess, DateTime lastUsingTime)
		{
			public string ContainerName { get; } = containerName;
			public Process AttachProcess { get; } = attachProcess;
			public DateTime LastUsingTime { get; set; } = lastUsingTime;
			public DataReceivedEventHandler? Handler { get; set; }
		}

		private readonly ushort _maxConnectionsAmount = 21000;
		private int _currentRecordsAmount = 0;
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
		public uint GetConnectionId(string hubConnectionId)
		{
			_connectionIds.TryGetValue(hubConnectionId, out var info);
			return info;
		}
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
					_connections[_id] = new ProcessInfo(connInfo.ContainerName, connInfo.AttachProcess, connInfo.LastUsingTime);
					SetConnectionId(connInfo.HubConnectionId, _id);
				}
			}
			catch (OverflowException)
			{
				_id = 1;
				_connections.Clear();
				_connectionIds.Clear();
				_connections[_id] = new ProcessInfo(connInfo.ContainerName, connInfo.AttachProcess, connInfo.LastUsingTime);
				SetConnectionId(connInfo.HubConnectionId, _id);
			}

			_currentRecordsAmount = Interlocked.Increment(ref _currentRecordsAmount);
			_id = Interlocked.Increment(ref _id);
			return _id - 1;
		}

		public bool RemoveHubConnection(string hubId)
		{
			var isRemoved2 = _connectionIds.TryRemove(hubId, out _);
			return isRemoved2;
		}

		public bool RemoveConnection(string hubId)
		{
			var hubExists = _connectionIds.TryGetValue(hubId, out var processId);
			if (!hubExists) return false;

			var isServerRemoved = RemoveServerConnection(hubId, out var proc);
			var isHubRemoved = isServerRemoved && RemoveHubConnection(hubId);
			if (!isHubRemoved)
				_connections[processId] = proc;
			else
				_currentRecordsAmount = Interlocked.Decrement(ref _currentRecordsAmount);
			return isHubRemoved;
		}
		private bool RemoveServerConnection(string hubId, out ProcessInfo processInfo)
		{
			_connectionIds.TryGetValue(hubId, out var processId);

			_connections[processId].AttachProcess.Kill();
			var isProcRemoved = _connections.TryRemove(processId, out processInfo);
			return isProcRemoved;
		}
		private void RemoveIrreleventRecords()
		{
			var keys = _connectionIds.Where(connId => _connections[connId.Value].LastUsingTime < DateTime.UtcNow - TimeSpan.FromMinutes(10))
					.Select(conn => conn.Key)
					.ToList();
			foreach (var key in keys)
				RemoveConnection(key);
		}
	}
}
