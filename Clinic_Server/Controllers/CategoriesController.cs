using Clinic_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Clinic_Server.Services;

namespace Clinic_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
       
        private readonly ILogger<CategoriesController> _logger;
        CategoryService categoryService;
        public CategoriesController(ILogger<CategoriesController> logger, CategoryService categoryService)
        {
            this._logger = logger;
            this.categoryService = categoryService;
        }

        [HttpGet("find-category")]
        async public Task<IActionResult> FindCategories([FromQuery] string? search, [FromQuery] string page)
        {
            try
            {
                var result = await this.categoryService.FindCategories(search, int.Parse(page));
                return StatusCode(200, new {result,page=int.Parse(page)});
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("all-categories")]
        async public Task<IActionResult> AllCategories()
        {
            try
            {
                var result = await this.categoryService.AllCategories();
                return StatusCode(200, new { result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
