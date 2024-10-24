namespace Clinic_Server.Data
{
    public class AddDbContext
    {
        protected string ConnectionString { set; get; }
        public AddDbContext(IConfiguration configuration)
        {
            //this.ConnectionString = configuration.GetConnectionString("OracleDatabase");
            this.ConnectionString = "User Id=sys;Password=jaba9293;Data Source=localhost:1521/xe;DBA Privilege=SYSDBA;";

        }
    }
}
