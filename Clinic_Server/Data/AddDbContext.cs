namespace Clinic_Server.Data
{
    public class AddDbContext
    {
        protected string ConnectionString { set; get; }
        public AddDbContext(IConfiguration configuration)
        {
            this.ConnectionString = configuration.GetConnectionString("OracleDatabase");
           // this.ConnectionString = "User Id=sys; Password=jaba9293; Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.8)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE))); DBA Privilege=SYSDBA;";
            

        }
    }
}
