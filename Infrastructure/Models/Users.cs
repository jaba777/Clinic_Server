

namespace Infrastructure.Models
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
        public int? category_id { get; set; }
        public byte[]? photo { get; set; }  
        public byte[]? resume { get; set; }  
        public string? otp {  get; set; }
        public Category? category { get; set; }
    }
}
