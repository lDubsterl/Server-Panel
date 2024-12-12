namespace Panel.Domain.Models
{
	public class UserAccount
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string PasswordSalt { get; set; }
		public string Role { get; set; } = "User";
		public bool MinecraftServer { get; set; } = false;
		public bool DSTServer { get; set; } = false;

	}
}
