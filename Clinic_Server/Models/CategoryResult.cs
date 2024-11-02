using Clinic_Server.Models;

namespace Clinic_Server.Services
{
    public class CategoryResult
    {
        public List<Category> Categories { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
