namespace Panel.Application.DTOs.AuthenticationRequests
{
    public class Token
    {
        public int UserId { get; set; }
        public string JwtToken { get; set; }
    }
}
