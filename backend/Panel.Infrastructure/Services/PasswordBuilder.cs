using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Panel.Infrastructure.Services
{
	public class PasswordBuilder
	{
		public static byte[] GetSecureSalt()
		{
			return RandomNumberGenerator.GetBytes(32);
		}
		public static string HashUsingPbkdf2(string password, byte[] salt)
		{
			byte[] derivedKey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iterationCount: 100000, 32);
			return Convert.ToBase64String(derivedKey);
		}
		public static string GeneratePassword(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}<>?";

			using var rng = RandomNumberGenerator.Create();
			var result = new char[length];
			var bytes = new byte[length];

			rng.GetBytes(bytes);

			for (int i = 0; i < length; i++)
			{
				result[i] = chars[bytes[i] % chars.Length];
			}

			return new string(result);
		}
	}
}
