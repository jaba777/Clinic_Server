namespace Clinic_Server.Models
{
    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }

        public int? user_count { get; set; }
    }
}
