using Panel.Application.Interfaces.Repositories;
using System.Collections;

namespace Panel.Infrastructure.Repositories
{
	public class UnitOfWork: IUnitOfWork
	{
		private Hashtable _repositories;
		public UnitOfWork()
		{
			if (_repositories == null)
				_repositories = [];
		}

		public IRepository<T> Repository<T>(string tableName) where T : class
		{

			var type = typeof(T).Name;

			if (!_repositories.ContainsKey(type))
			{
				var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(typeof(T)), tableName);
				_repositories[type] = repositoryInstance;
			}
			return (IRepository<T>)_repositories[type]!;
		}
	}
}
