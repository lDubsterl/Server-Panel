using Microsoft.Extensions.Configuration;
using Npgsql;
using Panel.Domain.DbConfigurations;
using System.Data;
using Dapper;

namespace Panel.Domain.Models
{
	public class RefreshToken
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string TokenHash { get; set; }
		public string TokenSalt { get; set; }
		public DateTime IssueDate { get; set; }
		public DateTime ExpiryDate { get; set; }

		public static async Task Create(IConfiguration config, IDbConfiguration<NpgsqlConnection> dbConfig)
		{
			using (IDbConnection conn = new NpgsqlConnection(config["DatabaseConnectionString"]))
			{
				var sqlQuery = """
					CREATE TABLE RefreshTokens(
					Id SERIAL PRIMARY KEY,
					UserId INTEGER REFERENCES UserAccounts(Id),
					TokenHash CHARACTER VARYING NOT NULL,
					TokenSalt CHARACTER VARYING NOT NULL,
					IssueDate TIMESTAMP WITH TIME ZONE NOT NULL,
					ExpiryDate TIMESTAMP WITH TIME ZONE NOT NULL)
					""";
				await conn.ExecuteAsync(sqlQuery);
			}
		}
	}
}
