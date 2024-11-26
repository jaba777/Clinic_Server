using Clinic_Server.Data;
using Clinic_Server.Helper;
using Clinic_Server.Models;
using Clinic_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private AuthHelper authHelper;
        private IRedisService redisService;
        private EmailService emailService;
        private readonly ILogger<UsersController> _logger;
        private UsersService usersService;
        public UsersController(AuthHelper authHelper, IRedisService redisService, EmailService emailService, ILogger<UsersController> logger, UsersService usersService)
        {
    
            this.authHelper = authHelper;
            this.redisService = redisService;
            this.emailService = emailService;
            this._logger = logger;
            this.usersService = usersService;
        }

        [HttpGet("my-profile")]
        [Authorize]
        async public Task<IActionResult> MyProfile(int userId)
        {
            try
            {
                var my_profile = await this.usersService.MyProfile(userId);
                return StatusCode(200, new { my_profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpGet("get-doctors")]
        async public Task<IActionResult> GetDoctors(int categoryId, int page)
        {
            try
            {
                var result =await this.usersService.GetDoctors(categoryId, page);
                return StatusCode(200, new { my_profile = result.users, totalPages = result.TotalPages, totalCount = result.TotalCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpGet("get-doctor")]
        async public Task<IActionResult> GetUser(int userId)
        {
            try
            {
                var result = await this.usersService.GetDoctor(userId);
                return StatusCode(200, new { user = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpPut("edit-user/{userId}")]
        [Authorize]
        async public Task<IActionResult> updateUser([FromForm] Doctor request, int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);

                if (verifiedToken.role != "admin")
                {
                    return StatusCode(405, new { success = false, message = "დაფიქსირდა შეცდომა" });
                } 
                var update_user = await this.usersService.EditUser(request, userId);
                return StatusCode(200, new { success = update_user, message = "მონაცემები წარმატებით შეიცვალა" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpDelete("delete-user/{userId}")]
        [Authorize]
        async public Task<IActionResult> DeleteUser(int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);

                if (verifiedToken.role != "admin")
                {
                    return StatusCode(405, new { success = false, message = "დაფიქსირდა შეცდომა" });
                }


                var delete_user = await this.usersService.DeleteUser(userId);
                return StatusCode(200, new { success = delete_user, message = "ექაუნთი წარმატებით წაიშალა" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpPost("verify-email")]
        async public Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
           
            try
            {
              var verifyEmail=await this.usersService.VerifyEmail(request);

                return StatusCode(200, new { success = verifyEmail, message = "შეამოწმეთ თქვენი მეილი" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }

        }
        [HttpPost("verify-otp")]
        async public Task<IActionResult> VerifyOtp([FromBody] VerifyRequest request)
        {
            try
            {
                var verifyOtp=await this.usersService.VerifyOtp(request);
                return StatusCode(200, new { success = verifyOtp, message = "კოდი სწორია" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }

        }
        [HttpPost("change-password")]
        async public Task<IActionResult> ChangePassword([FromBody] ChangePass request)
        {
            try
            {
                var changePassword = await this.usersService.ChangePassword(request);

                return StatusCode(200, new { success = changePassword, message = "პაროლი წარმატებით შეიცვალა" });
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { success = false, mesage = ex.Message });
            }
        }
    }
}
