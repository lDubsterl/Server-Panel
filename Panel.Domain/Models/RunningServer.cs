using Panel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Domain.Models
{
	public class RunningServer
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string ContainerName { get; set; }
		public int Port { get; set; }
		public ServerTypes ServerType { get; set; }
	}
}
