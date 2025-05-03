namespace Panel.Domain.Models
{
	public class User: AbstractEntity
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string PasswordSalt { get; set; }
		public string FtpPassword { get; set; }
		public string Role { get; set; } = "User";
		public string MinecraftServerExecutable { get; set; } = "";
		public bool DSTServer { get; set; } = false;
		public bool TerrariaServer { get; set; } = false;
		public DateTime Ts { get; set; }
		public List<RefreshToken> RefreshTokens { get; set; }
		public List <RunningServer> RunningServers { get; set; }

	}
}