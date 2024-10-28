using Clinic_Server.Models;
using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Asn1.Ocsp;
using StackExchange.Redis;
using System.Data;
using System.Xml.Linq;

namespace Clinic_Server.Data
{
    public class USER_PKG : AddDbContext
    {
        OracleConnection conn { get; set; }
        OracleCommand cmd { get; set; }
        public USER_PKG(IConfiguration config) : base(config)
        {
            this.conn = new OracleConnection();
            this.conn.ConnectionString = ConnectionString;
        }

        public bool Auth(Users user)
        {

            try
            {
                this.cmd = new OracleCommand();
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

        public bool DoctorAuth(Users user)
        {

            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_USERS.save_doctor";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = user.name;
                this.cmd.Parameters.Add("p_surname", OracleDbType.Varchar2).Value = user.surname;
                this.cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = user.email;
                this.cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = user.password;
                this.cmd.Parameters.Add("p_private_number", OracleDbType.Varchar2).Value = user.private_number;
                this.cmd.Parameters.Add("p_specialty_number", OracleDbType.Int32).Value = user.category_id;
                this.cmd.Parameters.Add("p_photo", OracleDbType.Blob).Value = user.photo;
                this.cmd.Parameters.Add("p_resume", OracleDbType.Blob).Value = user.resume;
                this.cmd.ExecuteNonQuery();
                this.conn.Close();

                return true;

            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Oracle error: {ex.Message}");

            }

        }
        public Users FindUser(string email)
        {

            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_USERS.find_user";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                this.cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                var reader = this.cmd.ExecuteReader();
                Users user = null;
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
                        user.password = reader["PASSWORD"].ToString();
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

  

        public List<Users> FindDoctors(int categoryId,int page)
        {
            var users = new List<Users>();
            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "CATEGORY_PKG.get_doctors";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_category", OracleDbType.Int32).Value = categoryId;
                this.cmd.Parameters.Add("p_page", OracleDbType.Int32).Value = page;
                this.cmd.Parameters.Add("o_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                this.cmd.Parameters.Add("o_total_pages", OracleDbType.Int32).Direction = ParameterDirection.Output;
                this.cmd.Parameters.Add("o_total_count", OracleDbType.Int32).Direction = ParameterDirection.Output;

                var reader = this.cmd.ExecuteReader();
               // t.id,t.name,t.surname,t.email,t.photo,t.resume
              
                    
                    while (reader.Read())
                    {
                        var user = new Users
                        {
                            id = reader["ID"] != DBNull.Value ? Convert.ToInt32(reader["ID"]) : 0,
                            name = reader["NAME"].ToString(),
                            surname = reader["SURNAME"].ToString(),
                            email = reader["EMAIL"].ToString(),
                            photo = reader["PHOTO"] != DBNull.Value ? (byte[])reader["PHOTO"] : null,
                            resume = reader["RESUME"] != DBNull.Value ? (byte[])reader["RESUME"] : null   
                        };
                        users.Add(user);
                    }
           
                
            }
            catch (Exception ex) {
                throw new ArgumentException($"Oracle error: {ex.Message}");
            }

            this.conn.Close();
            return users;

        }

        public Users Myprofile(int userId)
        {
            Users user = null;
            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_USERS.my_profile";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                this.cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var reader = this.cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        user = new Users();
                        while (reader.Read())
                        {
                            user.id = reader["ID"] != DBNull.Value ? int.Parse(reader["ID"].ToString()) : 0;
                            user.name = reader["NAME"]?.ToString();
                            user.email = reader["EMAIL"]?.ToString();
                            user.surname = reader["SURNAME"]?.ToString();
                            user.photo = reader["PHOTO"] != DBNull.Value ? (byte[])reader["PHOTO"] : null;
                            user.resume = reader["RESUME"] != DBNull.Value ? (byte[])reader["RESUME"] : null;
                            user.role = reader["ROLE"]?.ToString();
                            user.category = new Category
                            {
                                id = reader["CATID"] != DBNull.Value ? int.Parse(reader["CATID"].ToString()) : 0,
                                name = reader["CATNAME"]?.ToString()
                            };
                        }
                    }
                    else
                    {
                        Console.WriteLine("No data found for the given userId.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Oracle error: {ex.Message}");
                return null;
            }
            this.conn.Close();
            return user;

        }
    }
}
