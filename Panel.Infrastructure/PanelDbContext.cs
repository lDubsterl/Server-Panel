using Microsoft.EntityFrameworkCore;
using Panel.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Infrastructure
{
	public class PanelDbContext: DbContext
	{
		public DbSet<User> Users => Set<User>();
		public DbSet<RefreshToken> Tokens => Set<RefreshToken>();
		public DbSet<RunningServer> RunningServers => Set<RunningServer>();
		public PanelDbContext(DbContextOptions<PanelDbContext> options) : base(options)
		{
			Database.EnsureCreated();
		}
	}
}
