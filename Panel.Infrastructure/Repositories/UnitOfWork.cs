using Microsoft.Extensions.Caching.Distributed;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Collections;

namespace Panel.Infrastructure.Repositories
{
    public class UnitOfWork: IUnitOfWork
	{
		private PanelDbContext _context;
		//private IDistributedCache _cache;
		private Hashtable _repositories;
		public UnitOfWork(PanelDbContext context/*, IDistributedCache cache*/)
		{
			_context = context;
			_repositories ??= [];
			//_cache = cache;
		}

		public IRepository<T> Repository<T>() where T : AbstractEntity
		{

			var type = typeof(T).Name;

			if (!_repositories.ContainsKey(type))
			{
				//var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(typeof(T)), _context);
				_repositories[type] = new Repository<T>(_context/*, _cache*/);
			}
			return (IRepository<T>)_repositories[type]!;
		}
		public async Task<int> Save()
		{
			return await _context.SaveChangesAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
