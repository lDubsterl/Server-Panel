using Panel.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel.Application.Interfaces.Repositories
{
	public interface IUnitOfWork : IDisposable
	{
		public IRepository<T> Repository<T>() where T : class, IEntity;
		public Task<int> Save();
	}
}
