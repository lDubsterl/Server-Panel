using Npgsql;
using Panel.Domain.DbConfigurations;
using System.Data;

namespace Panel.Infrastructure.DbConfigurations
{
	public class PostgresConfiguration(IConfiguration config) : IDbConfiguration<NpgsqlConnection>, IDisposable
	{
		private NpgsqlConnection? _connection;
		public NpgsqlConnection Connection
		{
			get
			{
				if (_connection == null || _connection.State == ConnectionState.Closed)
				{
					_connection = new NpgsqlConnection(config["DatabaseConnectionString"]);
					_connection.Open();
				}
				return _connection;
			}
		}

		public void Dispose() => _connection?.Dispose();
	}
}
