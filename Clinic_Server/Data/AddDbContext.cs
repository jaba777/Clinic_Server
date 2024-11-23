namespace Clinic_Server.Data
{
    public class AddDbContext
    {
        protected string ConnectionString { set; get; }
        public AddDbContext(IConfiguration configuration)
        {
            this.ConnectionString = configuration.GetConnectionString("OracleDatabase");
        }
    }
}
