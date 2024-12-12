namespace Panel.Domain.Interfaces.Repositories
{
	public interface IRepository<T> where T : class
	{
		public Task<T> GetByIdAsync(int id);
		public Task<T> GetByFieldAsync(string fieldName, object fieldValue);
		public Task<IEnumerable<T>> GetAllAsync();
		public Task AddAsync(T entity);
		public Task UpdateAsync(T entity);
		public Task DeleteAsync(int recordId);
	}
}
