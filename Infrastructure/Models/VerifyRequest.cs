namespace Infrastructure.Models
{
    public class VerifyRequest
    {
        public string email { get; set; }
        public string otp { get; set; }
    }
}
