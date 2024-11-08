namespace Clinic_Server.Models
{
    public class Booking
    {
        public int? id { get; set; }
       
        public string ?time { get; set; }
        public string? date { get; set; }  
        public int ?user_id { get; set; }
        public int ?doctor_id  { get; set; }
        public int? day { get; set; }
    }
}
