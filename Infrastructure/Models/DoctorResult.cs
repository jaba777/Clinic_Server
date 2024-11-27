namespace Infrastructure.Models
{
    public class DoctorResult
    {
        public List<Users> users { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
