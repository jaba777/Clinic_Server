using Clinic_Server.Data;
using Clinic_Server.Helper;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        USER_PKG user_pkg;
        private AuthHelper authHelper;
        public UsersController(USER_PKG user_pkg, AuthHelper authHelper)
        {
            this.user_pkg = user_pkg;
            this.authHelper = authHelper;
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
                var result = this.user_pkg.FindDoctors( categoryId,page);
                return StatusCode(200, new { my_profile=result.users, totalPages=result.TotalPages, totalCount=result.TotalCount });
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
                return StatusCode(200, new {user=result  });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("edit-user/{userId}")]
        [Authorize]
        async public Task<IActionResult> updateUser([FromForm] Doctor request,int userId)
        {
            var access_token = HttpContext.Request.Headers["Authorization"].ToString()?.Substring("Bearer ".Length).Trim();
            try
            {
                var verifiedToken = authHelper.VerifyJWTToken(access_token);

                if (verifiedToken.role != "admin")
                {
                    return StatusCode(405,new {success=false,message="დაფიქსირდა შეცდომა"});
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

                if (request?.photo!=null)
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
                var update_user = this.user_pkg.UserUpdate(user,userId);
                return StatusCode(200, new {success= update_user,message="მონაცემები წარმატებით შეიცვალა" });
            }catch(Exception ex)
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


    }
}
