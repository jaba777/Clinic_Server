using Clinic_Server.Data;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Server.Services
{
    public class CategoryService
    {
        CATEGORY_PKG category_pkg;
        public CategoryService(CATEGORY_PKG category_pkg) {
            this.category_pkg = category_pkg;
        }
        async public Task<CategoryResult> FindCategories(string? search,int page)
        {
            return this.category_pkg.FindCategory(search, page);
        }

        async public Task<List<Category>> AllCategories()
        {
            return this.category_pkg.AllCategory();
        }

    }
}
