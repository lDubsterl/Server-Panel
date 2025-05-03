using Panel.Domain.Common;

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
