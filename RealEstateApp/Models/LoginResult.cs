namespace RealEstateApp.Models
{
    public class LoginResult
    {
        public bool Succeded { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
