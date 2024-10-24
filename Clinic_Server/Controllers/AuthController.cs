using Clinic_Server.Data;
using Clinic_Server.Helper;
using Clinic_Server.Models;
using Clinic_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
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

                return StatusCode(200, new { success = true, token, userId = finduser.id, role = finduser.role,message="success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
