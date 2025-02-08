﻿using Microsoft.EntityFrameworkCore;
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
		public DbSet<UserAccount> Users => Set<UserAccount>();
		public DbSet<RefreshToken> Tokens => Set<RefreshToken>();
		public DbSet<RunningServer> RunningServers => Set<RunningServer>();
	}
}
