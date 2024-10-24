

namespace Clinic_Server.Models
{
    public class Users
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? surname { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? role { get; set; }
        public string? private_number { get; set; }
        public byte[]? Photo { get; set; }  
        public byte[]? Resume { get; set; }  
        public string? otp {  get; set; }
    }
}
