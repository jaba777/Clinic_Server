using Clinic_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        CATEGORY_PKG category_pkg;
        public CategoriesController(CATEGORY_PKG category_pkg)
        {
            this.category_pkg = category_pkg;
        }

        [HttpGet("find-category")]
        async public Task<IActionResult> FindCategories([FromQuery] string? search, [FromQuery] string page)
        {
            try
            {

                var result = this.category_pkg.FindCategory(search, int.Parse(page));
                return StatusCode(200, new {result,page=int.Parse(page)});
            }
            catch (Exception ex) { 
            return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("all-categories")]
        async public Task<IActionResult> AllCategories()
        {
            try
            {

                var result = this.category_pkg.AllCategory();
                return StatusCode(200, new { result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
