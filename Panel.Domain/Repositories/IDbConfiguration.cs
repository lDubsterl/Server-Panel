using System.Data;

namespace Panel.Domain.DbConfigurations
{
	public interface IDbConfiguration<T> where T: IDbConnection
	{
		public T Connection { get; }
	}
}
