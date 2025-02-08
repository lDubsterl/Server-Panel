using Panel.Domain.Interfaces.Repositories;
using System.Collections;

namespace Panel.Infrastructure.Repositories
{
	public class UnitOfWork: IUnitOfWork
	{
		private PanelDbContext _context;
		private Hashtable _repositories;
		public UnitOfWork(PanelDbContext context)
		{
			_context = context;
			if (_repositories == null)
				_repositories = [];
		}

		public IRepository<T> Repository<T>() where T : class
		{

			var type = typeof(T).Name;

			if (!_repositories.ContainsKey(type))
			{
				var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(typeof(T)), _context);
				_repositories[type] = repositoryInstance;
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
