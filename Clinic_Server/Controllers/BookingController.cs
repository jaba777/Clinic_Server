using Clinic_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Clinic_Server.Helper;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        BOOKING_PKG booking_pkg;
        private AuthHelper authHelper;
        public BookingController(BOOKING_PKG booking_pkg, AuthHelper authHelper)
        {
            this.booking_pkg = booking_pkg;
            this.authHelper = authHelper;
        }

        [HttpPost("add-booking")]
        [Authorize]
        async public Task<IActionResult> CreateBook(Booking booking)
        {
            try
            {
                var findBook = this.booking_pkg.FindBooking(booking);
                if (findBook.id != null) {
                    return StatusCode(401, new {success=false,message= "ეს დრო უკვე დაჯავშნილი გაქვთ" });
                }
                var createBook = this.booking_pkg.AddBooking(booking);
                if (createBook == null)
                {
                    return StatusCode(401,new {success=false, message= "დაფიქსირდა შეცდომა" });
                }

                return StatusCode(200, new { book= createBook, success=true,message="დაჯავშნა წარმატებულია" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false,message = ex.Message});
            }
        }

        [HttpGet("get-books")]
        async public Task<IActionResult> GetBookings([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int doctorId)
        {
            try
            {
                var books=this.booking_pkg.GetBooks(startDate, endDate,doctorId);

                return StatusCode(200, new { books, success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpDelete("remove-book/{bookId}")]
        [Authorize]
        async public Task<IActionResult> RemoveBook([FromQuery] int userId,int bookId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);
                if (verifiedToken.userId != userId)
                {
                    return StatusCode(401, new { message = "something went wrong", success = false });
                }
                var deleteBook=booking_pkg.DeleteBook(userId, bookId);
                if (deleteBook != true)
                {
                    return StatusCode(401, new { message = deleteBook, success = false });
                }

                return StatusCode(200, new { message= "booking deleted successfully",success= deleteBook });
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
