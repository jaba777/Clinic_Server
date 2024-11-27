namespace Infrastructure.Models
{
    public class Signin
    {
        public string email { get; set; }
        public string password { get; set; }
    }
    public class GoogleLoginDto
    {
        public string idToken { get; set; }
    }
}
