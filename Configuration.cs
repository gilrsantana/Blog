namespace Blog;

public static class Configuration
{
    // Token - JWT - Jason Web Token
    public static string JwtKey  = null!;
    public static string ApiKeyName = null!;
    public static string ApiKey = null!;

    public static SmtpConfiguration Smtp { get; set; } = new();
    
    public class SmtpConfiguration
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; } 
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}