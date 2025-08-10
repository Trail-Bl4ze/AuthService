namespace Auth.Core.Models
{
    public class AuthenticationSettings
    {
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }
    }
}