using Clinic_Server.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Org.BouncyCastle.Asn1.Ocsp;
using StackExchange.Redis;
using System.Data;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Clinic_Server.Data
{
    public class BOOKING_PKG : AddDbContext
    {
        OracleConnection conn { get; set; }
        OracleCommand cmd { get; set; }
        public BOOKING_PKG(IConfiguration config) : base(config)
        {
            this.conn = new OracleConnection();
            this.conn.ConnectionString = ConnectionString;
        }




        public Booking  AddBooking(Booking booking)
        {
            Booking book= new Booking();
            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_BOOKING.Add_Booking";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_time", OracleDbType.Varchar2).Value = booking.time;
                this.cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = booking.user_id;
                this.cmd.Parameters.Add("p_doctor_id", OracleDbType.Int32).Value = booking.doctor_id;
                this.cmd.Parameters.Add("p_date", OracleDbType.Varchar2).Value = booking.date;
                this.cmd.Parameters.Add("p_booking_cursor", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var reader = this.cmd.ExecuteReader();
                while (reader.Read())
                {

                    book.id = int.Parse(reader["ID"].ToString());
                    book.time = reader["TIME"].ToString();
                    book.user_id = int.Parse(reader["USER_ID"].ToString());
                    book.doctor_id = int.Parse(reader["DOCTOR_ID"].ToString());
                    book.day = DateTime.Parse(reader["BOOK_DATE"].ToString()).Day;
                    
                }

                    this.conn.Close();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Oracle error: {ex.Message}");
            }
            return book;
        }

        public List<Booking> GetBooks(string startDate, string endDate, int doctorId)
        {
            var books = new List<Booking>();
            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_BOOKING.get_books";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_startDate", OracleDbType.Varchar2).Value = startDate;
                this.cmd.Parameters.Add("p_endDate", OracleDbType.Varchar2).Value = endDate;
                this.cmd.Parameters.Add("p_doctorId", OracleDbType.Int32).Value = doctorId;
                this.cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                var reader = this.cmd.ExecuteReader();
                while (reader.Read())
                {
                    var book = new Booking{
                        id = int.Parse(reader["ID"].ToString()),
                        time = reader["TIME"].ToString(),
                        user_id = int.Parse(reader["USER_ID"].ToString()),
                        doctor_id = int.Parse(reader["DOCTOR_ID"].ToString()),
                        day = DateTime.Parse(reader["BOOK_DATE"].ToString()).Day
                    };
                    books.Add(book);
                }
            }
            catch (Exception ex) {

                throw new ArgumentException($"Oracle error: {ex.Message}");
            }
            this.conn.Close();
            return books;
        }

        public bool DeleteBook(int userId,int bookId)
        {
            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_BOOKING.delete_book";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                this.cmd.Parameters.Add("p_bookId", OracleDbType.Int32).Value = bookId;
                this.cmd.ExecuteNonQuery();
                this.conn.Close();
            }
            catch (Exception ex) {
                throw new ArgumentException($"Oracle error: {ex.Message}");
            }
            return true;
        }


        // p_date varchar2, p_time varchar2,p_user_id int, p_result OUT SYS_REFCURSOR

        public Booking FindBooking(Booking booking)
        {
            Booking book = new Booking();
            try
            {
                this.cmd = new OracleCommand();
                this.conn.Open();
                this.cmd.Connection = this.conn;
                this.cmd.CommandText = "PKG_BOOKING.find_book";
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.Parameters.Add("p_date", OracleDbType.Varchar2).Value = booking.date;
                this.cmd.Parameters.Add("p_time", OracleDbType.Varchar2).Value = booking.time;
                this.cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = booking.user_id;
                this.cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                var reader = this.cmd.ExecuteReader();
                while (reader.Read())
                {

                    book.id = int.Parse(reader["ID"].ToString());
                    book.time = reader["TIME"].ToString();
                    book.user_id = int.Parse(reader["USER_ID"].ToString());
                    book.doctor_id = int.Parse(reader["DOCTOR_ID"].ToString());
                    book.day = DateTime.Parse(reader["BOOK_DATE"].ToString()).Day;

                }

                this.conn.Close();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Oracle error: {ex.Message}");
            }
            return book;
        }


    }
}
