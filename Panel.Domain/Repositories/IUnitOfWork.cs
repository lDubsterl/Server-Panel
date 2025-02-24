using Panel.Domain.Models;

namespace Panel.Domain.Interfaces.Repositories
{
	public interface IUnitOfWork
	{
		public IRepository<T> Repository<T>() where T : AbstractEntity;
		public Task<int> Save();
	}
}
