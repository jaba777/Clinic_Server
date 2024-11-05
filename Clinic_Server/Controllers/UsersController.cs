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
        USER_PKG user_pkg;
        private AuthHelper authHelper;
        private IRedisService redisService;
        private EmailService emailService;
        public UsersController(USER_PKG user_pkg, AuthHelper authHelper, IRedisService redisService, EmailService emailService)
        {
            this.user_pkg = user_pkg;
            this.authHelper = authHelper;
            this.redisService = redisService;
            this.emailService = emailService;
        }

        [HttpGet("my-profile")]
        [Authorize]
        async public Task<IActionResult> Register(int userId)
        {
            try
            {
                var my_profile = this.user_pkg.Myprofile(userId);
                return StatusCode(200, new { my_profile });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-doctors")]
        async public Task<IActionResult> GetDoctors(int categoryId, int page)
        {
            try
            {
                var result = this.user_pkg.FindDoctors(categoryId, page);
                return StatusCode(200, new { my_profile = result.users, totalPages = result.TotalPages, totalCount = result.TotalCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-doctor")]
        async public Task<IActionResult> GetUser(int userId)
        {
            try
            {
                var result = this.user_pkg.getUser(userId);
                return StatusCode(200, new { user = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
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

                if (!string.IsNullOrEmpty(request?.password))
                {
                    var regexPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?""':;{}|<>]).{8,16}$";

                    if (!Regex.IsMatch(request.password, regexPattern))
                    {
                        return StatusCode(401, new { success = false, message = "პაროლი არავალიდურია" });
                    }
                    else
                    {
                        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);
                        request.password = passwordHash;
                    }
                }
                else
                {
                    request.password = "";
                }


                byte[] photoBytes = null;
                byte[] resumeBytes = null;

                if (request?.photo != null)
                {
                    var ms = new MemoryStream();
                    await request.photo.CopyToAsync(ms);
                    photoBytes = ms.ToArray();
                }
                if (request?.resume != null)
                {
                    var ms = new MemoryStream();
                    await request.resume.CopyToAsync(ms);
                    resumeBytes = ms.ToArray();
                }
                var user = new Users
                {
                    name = request.name,
                    surname = request.surname,
                    email = request.email,
                    private_number = request.private_number,
                    password = request.password,
                    category_id = int.Parse(request.category_id),
                    photo = photoBytes,
                    resume = resumeBytes,
                };
                var update_user = this.user_pkg.UserUpdate(user, userId);
                return StatusCode(200, new { success = update_user, message = "მონაცემები წარმატებით შეიცვალა" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete-user/{userId}")]
        [Authorize]
        async public Task<IActionResult> DeleteUser([FromForm] Doctor request, int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);

                if (verifiedToken.role != "admin")
                {
                    return StatusCode(405, new { success = false, message = "დაფიქსირდა შეცდომა" });
                }


                var delete_user = this.user_pkg.UserDelete(userId);
                return StatusCode(200, new { success = delete_user, message = "ექაუნთი წარმატებით წაიშალა" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("verify-email")]
        async public Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var redisDb = redisService.GetDatabase();
            try
            {
                var finduser = user_pkg.FindUser(request.email);
                if (finduser == null)
                {
                    return StatusCode(401, new { success = false, message = "ექაუნთი ვერ მოიძებნა" });
                }


                var userCache = await redisDb.StringGetAsync($"user_email:{request.email}");
                if (userCache.HasValue)
                {
                    await redisDb.KeyDeleteAsync($"user_email:{request.email}");
                }

                Random random = new Random();
                string otp = random.Next(1000, 10000).ToString();
                var verify = new OtpData { otp = otp, verify = false };
                await redisDb.StringSetAsync($"user_email:{request.email}", JsonSerializer.Serialize(verify), TimeSpan.FromMinutes(2));
                await this.emailService.SendEmailOtp(request.email, otp, finduser.name);

                return StatusCode(200, new { success = true, message = "შეამოწმეთ თქვენი მეილი" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mesage = ex.Message });
            }

        }
        [HttpPost("verify-otp")]
        async public Task<IActionResult> VerifyOtp([FromBody] VerifyRequest request)
        {
            var redisDb = redisService.GetDatabase();
            try
            {
                var userCache = await redisDb.StringGetAsync($"user_email:{request.email}");
                if (userCache.IsNull)
                {
                    return StatusCode(402, new { success = false, message = "დრო ამოიწურა, სცადეთ თავიდან" });
                }

                var getOtp = JsonSerializer.Deserialize<OtpData>(userCache.ToString());
                if (getOtp.otp != request.otp)
                {
                    return StatusCode(402, new { success = false, message = "კოდი არასწორია" });
                }

                await redisDb.KeyDeleteAsync($"user_email:{request.email}");

                var verify = new OtpData { otp = request.otp, verify = true };
                await redisDb.StringSetAsync($"user_email:{request.email}", JsonSerializer.Serialize(verify), TimeSpan.FromMinutes(5));


                return StatusCode(200, new { success = true, message = "კოდი სწორია" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mesage = ex.Message });
            }

        }
        [HttpPost("change-password")]
        async public Task<IActionResult> ChangePassword([FromBody] ChangePass request)
        {
            var redisDb = redisService.GetDatabase();
            try
            {
                var userCache = await redisDb.StringGetAsync($"user_email:{request.email}");
                if (userCache.IsNull)
                {
                    return StatusCode(402, new { success = false, message = "დრო ამოიწურა, სცადეთ თავიდან" });
                }

                var getOtp = JsonSerializer.Deserialize<OtpData>(userCache.ToString());
                if (getOtp.verify != true)
                {
                    return StatusCode(402, new { success = false, message = "კოდი არ არის ვერიფიცირებული" });
                }
                var regexPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?""':;{}|<>]).{8,16}$";
                if (!Regex.IsMatch(request.password, regexPattern))
                {
                    return StatusCode(402, new { success = false, message = "პაროლი არავალიდურია" });
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

                var changePassword = user_pkg.ChangePassword(request.email, passwordHash);

                return StatusCode(200, new { success = changePassword, message = "პაროლი წარმატებით შეიცვალა" });
            }
            catch (Exception ex) {
                return StatusCode(500, new { success = false, mesage = ex.Message });
            }
        }
    }
}
