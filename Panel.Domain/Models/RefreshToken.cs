using Microsoft.Extensions.Configuration;
using Npgsql;
using Panel.Domain.DbConfigurations;
using System.Data;
using Dapper;

namespace Panel.Domain.Models
{
	public class RefreshToken: AbstractEntity
	{
		public int UserId { get; set; }
		public string TokenHash { get; set; }
		public string TokenSalt { get; set; }
		public DateTime IssueDate { get; set; }
		public DateTime ExpiryDate { get; set; }

	}
}
