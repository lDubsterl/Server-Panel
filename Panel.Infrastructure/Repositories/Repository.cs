using Dapper;
using Npgsql;
using Panel.Domain.DbConfigurations;
using Panel.Domain.Interfaces.Repositories;
using System.Data;
using System.Text;

namespace Panel.Infrastructure.Repositories
{
	public class Repository<T>(string tableName, IDbConfiguration<NpgsqlConnection> dbConfig) : IRepository<T> where T: class
	{
		private readonly NpgsqlConnection _db = dbConfig.Connection;
		public Task AddAsync(T entity)
		{
			var parameters = new DynamicParameters();
			StringBuilder query = new($"INSERT INTO {tableName} VALUES (");
			var fields = typeof(T).GetFields();

			for (int i = 1; i < fields.Length; i++)
			{
				parameters.Add($"@{fields[i].Name}", fields[i].GetValue(entity));
				query.Append($"{fields[i].Name}=@{fields[i].Name}, ");
			}

			query.Remove(query.Length - 2, 2);
			query.Append(')');

			_db.ExecuteAsync(query.ToString());
			return Task.CompletedTask;
		}

		public Task DeleteAsync(int id)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@id", id);
			return _db.ExecuteAsync($"DELETE FROM {tableName} where Id=@id", parameters);
		}

		public Task<IEnumerable<T>> GetAllAsync()
		{
			return _db.QueryAsync<T>($"SELECT * FROM {tableName}");
		}
		public Task<T> GetByFieldAsync(string fieldName, object fieldValue)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@field", fieldName);
			parameters.Add("@value", fieldValue);
			return _db.QueryFirstAsync<T>($"SELECT * FROM {tableName} WHERE @field=@value", parameters);
		}
		public Task<T> GetByIdAsync(int id)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@id", id);
			return _db.QueryFirstAsync<T>($"SELECT * FROM {tableName} WHERE Id=@id", parameters);
		}

		public Task UpdateAsync(T entity)
		{
			var parameters = new DynamicParameters();
			StringBuilder query = new($"UPDATE {tableName} set ");
			var fields = typeof(T).GetFields();

			for (int i = 1; i < fields.Length; i++)
			{
				parameters.Add($"@{fields[i].Name}", fields[i].GetValue(entity));
				query.Append($"{fields[i].Name}=@{fields[i].Name}, ");
			}

			query.Remove(query.Length - 2, 2);
			query.Append("where Id=@id");

			_db.ExecuteAsync(query.ToString());
			return Task.CompletedTask;
		}
	}
}
