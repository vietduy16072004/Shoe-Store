namespace Shoe.ViewModels
{
    public class SocialLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // "Google"
        public string ProviderId { get; set; } = string.Empty;
    }
}
