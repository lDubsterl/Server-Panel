using Npgsql;

namespace ServerPanel.Hub
{
	public class DbConnection
	{
		const string connectionString = "Server=127.0.0.1;User Id=postgres;Password=1;Port=5432;Database=SiteAccounts;";

		public NpgsqlConnection connection { get; }
		public DbConnection()
		{
			connection = new NpgsqlConnection(connectionString);
		}
	}
}
