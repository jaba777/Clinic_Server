using Clinic_Server.Data;
using Clinic_Server.Helper;
using Clinic_Server.Models;
using Clinic_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using System.Text.RegularExpressions;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        USER_PKG user_pkg;
        private readonly IConfiguration _configuration;
        private AuthHelper authHelper;
        private IRedisService redisService;
        private RegisterHelper registerHelper;
        public AuthController(USER_PKG user_pkg, IRedisService redisService, RegisterHelper registerHelper, AuthHelper authHelper)
        {
            this.user_pkg = user_pkg;
            this.redisService = redisService;
            this.registerHelper = registerHelper;
            this.authHelper = authHelper;
        }

        [HttpPost("sign-up")]
        async public Task<IActionResult> Register(Users request)
        {
            try
            {
                var finduser = user_pkg.FindUser(request.email);
                if (finduser != null)
                {
                    return BadRequest("This account is already created");
                }

                if (string.IsNullOrEmpty(request.otp))
                {
                    var registerUser = await this.registerHelper.Register(request);
                    return StatusCode(200, new { success = true,message="check Email",email=request.email, isRegistered = false });
                } else if (!string.IsNullOrEmpty(request.otp))
                {
                    Users verifyUser = await this.registerHelper.verifyOtp(request.email,request.otp);
                    if (verifyUser != null)
                    {
                        var createUser = this.user_pkg.Auth(verifyUser);
                        return StatusCode(200, new { success = createUser, message = "Registered successfully", email = request.email,isRegistered=true });
                    }
                    else
                    {
                        return BadRequest("OTP verification failed.");
                    }
                }

                return StatusCode(200, new { success = true, message = "Registere successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


        [HttpPost("sign-up-doctor")]
        [Authorize]
        async public Task<IActionResult> RegisterDoctor([FromForm] Doctor request, [FromQuery] int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);

                if (verifiedToken.role != "admin")
                {
                    return StatusCode(405, new { success = false, message = "დაფიქსირდა შეცდომა" });
                }

                if (string.IsNullOrEmpty(request.email))
                {
                    return BadRequest("Email is not valid");
                }
                var finduser = user_pkg.FindUser(request.email);
                if (finduser != null)
                {
                    return BadRequest("This account is already created");
                }

                byte[] photoBytes;
                byte[] resumeBytes;

                using (var ms = new MemoryStream())
                {
                    await request.photo.CopyToAsync(ms);
                    photoBytes = ms.ToArray();
                }

                using (var ms = new MemoryStream())
                {
                    await request.resume.CopyToAsync(ms);
                    resumeBytes = ms.ToArray();
                }
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

                var user = new Users 
                {
                    name = request.name,
                    surname = request.surname,
                    email = request.email,
                    private_number = request.private_number,
                    password = passwordHash,
                    category_id = int.Parse(request.category_id),
                    photo = photoBytes,
                    resume = resumeBytes
                };

                var result = this.user_pkg.DoctorAuth(user);

                if (result)
                {
                    return StatusCode(200, new { success = true, message = "Registere successfully" });
                }


                return StatusCode(200, new { success = false, message = "Registere not successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


        [HttpPost("sign-in")]
        async public Task<IActionResult> SignIn(Signin request)
        {
            try
            {
                Users finduser = user_pkg.FindUser(request.email);
                if (finduser == null)
                {
                    return StatusCode(401, new { success = false, message = "This email isn't registered" });
                }

                bool verified = BCrypt.Net.BCrypt.Verify(request.password, finduser.password);

                if (verified!=true)
                {
                    return StatusCode(401, new { success = false,message= "Invalid password" });
                }

                var token = authHelper.GenerateJWTToken(finduser);
                finduser.password = null;

                return StatusCode(200, new { success = true, token,user=finduser,message="success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
