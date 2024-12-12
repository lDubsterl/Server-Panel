namespace Panel.Domain.Interfaces.Repositories
{
	public interface IUnitOfWork
	{
		public IRepository<T> Repository<T>(string tableName) where T : class;
	}
}
