using Clinic_Server.Data;
using Clinic_Server.Helper;
using Clinic_Server.Models;
using Clinic_Server.Services;
using Microsoft.AspNetCore.Http;
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
        public AuthController(USER_PKG user_pkg, IRedisService redisService, RegisterHelper registerHelper)
        {
            this.user_pkg = user_pkg;
            this.redisService = redisService;
            this.registerHelper = registerHelper;
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
                    return StatusCode(200, new { success = true,message="check Email",email=request.email });
                } else if (!string.IsNullOrEmpty(request.otp))
                {
                    Users verifyUser = await this.registerHelper.verifyOtp(request.email,request.otp);
                   var createUser = this.user_pkg.Auth(verifyUser);
                    return StatusCode(200, new { success = createUser, message="Registere successfully" });
                }

                return StatusCode(200, new { success = true, message = "Registere successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
