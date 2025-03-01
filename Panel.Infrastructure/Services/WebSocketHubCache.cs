using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Infrastructure.Services
{
	public class WebSocketHubCache
	{
		public class ConnectionInfo(ClientWebSocket connection, DateTime usingTime)
		{
			public ClientWebSocket Conn { get; } = connection;
			public DateTime LastUsingTime { get; } = usingTime;

		}

		private readonly ushort _maxConnectionsAmount = 21000;
		private ushort _currentRecordsAmount = 0;
		private uint _id = 1;
		private readonly ConcurrentDictionary<uint, ConnectionInfo> _cache = new();

		public ClientWebSocket? GetConnection(uint id) => _cache[id]?.Conn;

		public uint AddConnection(ConnectionInfo connInfo)
		{
			if (_currentRecordsAmount >= _maxConnectionsAmount)
				RemoveIrreleventRecords();
			try
			{
				checked
				{
					_cache[_id++] = connInfo;
				}
			}
			catch (OverflowException)
			{
				_id = 1;
				_cache.Clear();
				_cache[_id++] = connInfo;
			}

			_currentRecordsAmount++;
			return _id;
		}

		private void RemoveIrreleventRecords()
		{
			var keys = _cache.Where(conn => conn.Value.LastUsingTime < DateTime.UtcNow - TimeSpan.FromMinutes(10))
					.Select(conn => conn.Key)
					.ToList();
			foreach (var key in keys)
			{
				_cache.TryRemove(key, out _);
				_currentRecordsAmount--;
			}
		}

		public bool RemoveConnection(uint id)
		{
			bool isRemoved = _cache.TryRemove(id, out _);
			if (isRemoved) _currentRecordsAmount--;
			return isRemoved;
		}
	}
}
