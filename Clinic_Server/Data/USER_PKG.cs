using Clinic_Server.Models;
using Oracle.ManagedDataAccess.Client;
using StackExchange.Redis;
using System.Data;
using System.Xml.Linq;

namespace Clinic_Server.Data
{
    public class USER_PKG: AddDbContext
    {
        OracleConnection conn { get; set; }
        OracleCommand cmd { get; set; }
        public USER_PKG(IConfiguration config):base(config) 
        {
            this.conn = new OracleConnection();
            this.conn.ConnectionString=ConnectionString;
            this.cmd = new OracleCommand();
        }

        public bool Auth(Users user)
        {

            try
            {

                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_USERS.save_users";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = user.name;
                this.cmd.Parameters.Add("p_surname", OracleDbType.Varchar2).Value = user.surname;
                this.cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = user.email;
                this.cmd.Parameters.Add("p_private_number", OracleDbType.Varchar2).Value = user.private_number;
                this.cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = user.password;
                this.cmd.ExecuteNonQuery();
                this.conn.Close();

                return true;

            }
            catch (Exception ex) {
                Console.WriteLine($"Oracle error: {ex.Message}");
                return false;
            }

        }
        public Users FindUser(string email)
        {
            
            try
            {

                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_USERS.find_user";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                this.cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                var reader = this.cmd.ExecuteReader();
                Users user=null;
                if (reader.HasRows) 
                {
                    user = new Users();
                    while (reader.Read())
                    {
                        user.id = int.Parse(reader["ID"].ToString()); 
                        user.name = reader["NAME"].ToString(); 
                        user.email = reader["EMAIL"].ToString();
                        user.surname = reader["SURNAME"].ToString();
                        user.private_number = reader["PRIVATE_NUMBER"].ToString();
                        user.role = reader["ROLE"].ToString();
                    }
                }
                this.conn.Close();
                return user; 
                

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Oracle error: {ex.Message}");
                return null;
            }
        }
    }
}
