using Panel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Domain.Models
{
	public class RunningServer: AbstractEntity
	{
		public int UserId { get; set; }
		public string ContainerName { get; set; }
		public int Port { get; set; }
		public ServerTypes ServerType { get; set; }
		public uint ConnectionId { get; set; } = 0;
	}
}
