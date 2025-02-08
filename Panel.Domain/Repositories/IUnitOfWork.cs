namespace Panel.Domain.Interfaces.Repositories
{
	public interface IUnitOfWork
	{
		public IRepository<T> Repository<T>() where T : class;
		public Task<int> Save();
	}
}
