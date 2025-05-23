namespace VeragWebApp.server.Helper
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string UserRole { get; set; }
        public int? TimasId { get; set; }
    }
}
