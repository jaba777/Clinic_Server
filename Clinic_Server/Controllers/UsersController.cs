using Clinic_Server.Data;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        USER_PKG user_pkg;
        public UsersController(USER_PKG user_pkg)
        {
            this.user_pkg = user_pkg;
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
                var my_profile = this.user_pkg.FindDoctors( categoryId,page);
                return StatusCode(200, new { my_profile });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}
