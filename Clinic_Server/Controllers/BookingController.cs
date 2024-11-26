using Clinic_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Clinic_Server.Helper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;
using Clinic_Server.Services;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private AuthHelper authHelper;
        private BookingService bookingService;
        private readonly ILogger<BookingController> _logger;
        public BookingController(AuthHelper authHelper, ILogger<BookingController> logger, BookingService bookingService)
        {
            this.authHelper = authHelper;
            _logger = logger;
            this.bookingService = bookingService;
        }

        [HttpPost("add-booking")]
        [Authorize]
        async public Task<IActionResult> CreateBook(Booking booking,int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);
                if (verifiedToken.userId != userId && verifiedToken.role != "user")
                {
                    return StatusCode(401, new { message = "დაფიქსირდა შეცდომა", success = false });
                }

                var createBook = await this.bookingService.AddBooking(booking, userId);
                return StatusCode(200, new { book = createBook, success = true, message = "დაჯავშნა წარმატებულია", booking });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("get-books")]
        async public Task<IActionResult> GetBookings([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int doctorId)
        {
            try
            {
                var books = await this.bookingService.Getbooks(startDate, endDate, doctorId);

                return StatusCode(200, new { books, success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("get-book-count")]
        async public Task<IActionResult> GetUsersBookings([FromQuery] int userId)
        {
            try
            {
                var books = await this.bookingService.GetBookCount(userId);

                return StatusCode(200, new { books, success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpDelete("remove-book/{bookId}")]
        [Authorize]
        async public Task<IActionResult> RemoveBook([FromQuery] int userId, int bookId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);
                if (verifiedToken.userId != userId && verifiedToken.role != "admin")
                {
                    return StatusCode(401, new { message = "დაფიქსირდა შეცდომა", success = false });
                }
                var deleteBook = await this.bookingService.RemoveBook(userId, bookId);
                if (deleteBook != true)
                {
                    return StatusCode(401, new { message = deleteBook, success = false });
                }

                return StatusCode(200, new { message = "ჯავშანი წარმატებით წაიშალა", success = deleteBook });
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpDelete("remove-books/{userId}")]
        [Authorize]
        async public Task<IActionResult> RemoveBooks([FromQuery] string startDate, [FromQuery] string endDate, int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);
                if (verifiedToken.userId != userId && verifiedToken.role != "admin")
                {
                    return StatusCode(401, new { message = "დაფიქსირდა შეცდომა", success = false });
                }
                var deleteBooks = await this.bookingService.RemoveBooks(startDate, endDate, userId);
                if (deleteBooks != true)
                {
                    return StatusCode(401, new { message = deleteBooks, success = false });
                }

                return StatusCode(200, new { message = "ჯავშანები წარმატებით წაიშალა", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpPut("update-book/{bookId}/{userId}")]
        [Authorize]
        async public Task<IActionResult> UpdateBook(int bookId, int userId,[FromBody] Booking booking, [FromQuery] int receiverId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);
                if (verifiedToken.userId != userId && verifiedToken.role!="admin")
                {
                    return StatusCode(401, new { message = "დაფიქსირდა შეცდომა", success = false });
                }
                var updateBooking = await this.bookingService.UpdateBook(bookId, userId,booking, receiverId);

                return StatusCode(200, new {success=true, book=updateBooking,message="ჯავშანი წარმატებით შეიცვალა", booking });
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success=false });
            }
        }
    }
}
