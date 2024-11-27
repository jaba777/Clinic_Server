
using Microsoft.Extensions.Configuration;
namespace Infrastructure.Data
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
