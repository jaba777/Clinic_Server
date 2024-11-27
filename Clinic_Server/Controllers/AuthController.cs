using Infrastructure.Data;
using Clinic_Server.Helper;
using Infrastructure.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using Infrastructure.Redis;

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
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private AuthService authService;

        public AuthController(USER_PKG user_pkg, IRedisService redisService, RegisterHelper registerHelper, AuthHelper authHelper, ILogger<AuthController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, AuthService authService)
        {
            this.user_pkg = user_pkg;
            this.redisService = redisService;
            this.registerHelper = registerHelper;
            this.authHelper = authHelper;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            this.authService = authService;
        }

        [HttpPost("sign-up")]
        async public Task<IActionResult> Register(Users request)
        {
            try
            {
                var finduser = user_pkg.FindUser(request.email);
                if (finduser != null)
                {
                    return StatusCode(401, new { message = "ამ მეილით ექაუნთი უკვე შექმნილია", success = false });
                }

                if (string.IsNullOrEmpty(request.otp))
                {
                    var registerUser = await this.registerHelper.Register(request);
                    return StatusCode(200, new { success = true, message = "შეამოწმეთ მეილი", email = request.email, isRegistered = false });
                }
                else if (!string.IsNullOrEmpty(request.otp))
                {
                    Users verifyUser = await this.registerHelper.verifyOtp(request.email, request.otp);
                    if (verifyUser != null)
                    {
                        var createUser = this.user_pkg.Auth(verifyUser);
                        return StatusCode(200, new { success = createUser, message = "რეგისტრაცია წარმატებულია", email = request.email, isRegistered = true });
                    }
                    else
                    {
                        return StatusCode(403, new { message = "ვერიფიკაცია წარუმატებელია, სცადეთ თავიდან", success = false });
                    }
                }

                return StatusCode(200, new { success = true, message = "რეგისტრაცია წარმატებულია" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
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

                var result = await this.authService.RegisterDoctor(request);

                if (result)
                {
                    return StatusCode(200, new { success = true, message = "რეგისტრაცია წარმატებულია" });
                }


                return StatusCode(500, new { success = false, message = "რეგისტრაცია წარუმატებელია" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }

        }


        [HttpPost("sign-in")]
        async public Task<IActionResult> SignIn(Signin request)
        {
            try
            {
                Users finduser = await this.authService.UserLogin(request);

                var token = authHelper.GenerateJWTToken(finduser);
              

                return StatusCode(200, new { success = true, token, user = finduser, message = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new { message = ex.Message, success = false });
            }

        }

        [HttpPost("google-login")]
        async public Task<IActionResult> GoogleLogin(GoogleLoginDto request)
        {
            try
            {
                var idToken = request.idToken;
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };
                var result = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                if(result is null)
                {
                    return StatusCode(402, new { message = "დაფიქსირდა შეცდომა", success = false });
                }

                Users finduser = user_pkg.FindUser(result.Email);
                if (finduser == null || finduser.id == null)
                {
                    return StatusCode(401, new { success = false, message = "მოცემული მეილი არ არის რეგისტრირებული" });
                }


                var token = authHelper.GenerateJWTToken(finduser);
                finduser.password = null;

                return StatusCode(200, new { success = true, token, user = finduser, message = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ID token.");
                return StatusCode(500, new { message = ex.Message, success = false });
            }

        }




    }
}
