using Panel.Domain.Interfaces;

namespace Panel.Application.Interfaces.Repositories
{
	public interface IRepository<T> where T : class, IEntity
	{
		public IQueryable<T> Entities { get; }
		public Task<T> GetByIdAsync(int id);
		public Task<IEnumerable<T>> GetAllAsync();
		public T Add(T entity);
		public Task UpdateAsync(T entity);
		public Task DeleteAsync(T entity);
	}
}
