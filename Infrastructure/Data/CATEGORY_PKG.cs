
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using Microsoft.Extensions.Configuration;
using Infrastructure.Models;

namespace Infrastructure.Data
{
    public class CATEGORY_PKG : AddDbContext
    {
        OracleConnection conn;
        OracleCommand cmd;
        public CATEGORY_PKG(IConfiguration config):base(config) {
            this.conn = new OracleConnection();
            this.conn.ConnectionString = ConnectionString;

        }

        public CategoryResult FindCategory(string search,int page)
        {
            List<Category> categories = new List<Category>();
            int totalPages = 0;
            int totalCount = 0;

            try
            {

                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "CATEGORY_PKG.get_categories";
                this.cmd.CommandType = CommandType.StoredProcedure;

                this.cmd.Parameters.Add("p_search", OracleDbType.Varchar2).Value = (object)search ?? DBNull.Value;
                this.cmd.Parameters.Add("p_page", OracleDbType.Int32).Value = page;
                this.cmd.Parameters.Add("o_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                this.cmd.Parameters.Add("o_total_pages", OracleDbType.Int32).Direction = ParameterDirection.Output;
                this.cmd.Parameters.Add("o_total_count", OracleDbType.Int32).Direction = ParameterDirection.Output;

                var reader = this.cmd.ExecuteReader();

                while (reader.Read()) {
                    Category category = new Category
                    {
                        id = int.Parse(reader["ID"].ToString()),
                        name = reader["NAME"].ToString()
                    };

                    categories.Add(category);
                }

                totalCount = (int)((OracleDecimal)this.cmd.Parameters["o_total_count"].Value).ToInt32();
                totalPages = (int)((OracleDecimal)this.cmd.Parameters["o_total_pages"].Value).ToInt32();
                this.conn.Close();
            }
            catch (Exception ex) {
                throw new Exception("An error occurred while retrieving categories: " + ex.Message);
            }

            return new CategoryResult
            {
                Categories = categories,
                TotalCount = totalCount,
                TotalPages = totalPages
            };


        }

        public List<Category> AllCategory()
        {
            List<Category> categories = new List<Category>();
            try
            {

                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "CATEGORY_PKG.get_all_categories";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("o_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var reader = this.cmd.ExecuteReader();

                while (reader.Read())
                {
                    Category category = new Category
                    {
                        id = int.Parse(reader["ID"].ToString()),
                        name = reader["NAME"].ToString(),
                        user_count= int.Parse(reader["user_count"].ToString())
                    };

                    categories.Add(category);
                }

                this.conn.Close();

            }
            catch (Exception ex) {
                throw new Exception("An error occurred while retrieving categories: " + ex.Message);
            }

            return categories;
           
        }
    }
}
