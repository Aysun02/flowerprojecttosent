using Fiorello.DAL;
using Fiorello.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fiorello.Models;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public CategoryController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CategoryIndexViewModel
            {
                Categories = await _appDbContext.Categories.ToListAsync()

            };

            return View(model);
        }
        #region Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            bool isExist = await _appDbContext.Categories.
                                               AnyAsync(c => c.Title.ToLower().Trim() == category.Title.ToLower().Trim());
            if (isExist)
            {
                ModelState.AddModelError("Title", "Bu Adda Category artiq movcuddur!");
                return View(category);
            }
            await _appDbContext.Categories.AddAsync(category);
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            var category = await _appDbContext.Categories.FindAsync(Id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComponent(int Id)
        {
            var Dbcategory = await _appDbContext.Categories.FindAsync(Id);
            if(Dbcategory == null) return NotFound();

            _appDbContext.Remove(Dbcategory);
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #endregion

        #region Update
        [HttpGet]
        public async Task<IActionResult> Update(int Id)
        {
            var category = await _appDbContext.Categories.FindAsync(Id);
            if(category==null) return NotFound();

            return View(category);
        }

        [HttpPost]
        
        public async Task<IActionResult> Update (int Id, Category category)
        {
            if (!ModelState.IsValid) return View(category);

            if (Id != category.Id) return BadRequest();

            var DbCategory = await _appDbContext.Categories.FindAsync(Id);
            if(DbCategory == null) return NotFound();

            bool isExist = await _appDbContext.Categories
                            .AnyAsync(c => c.Title.ToLower().Trim() == category.Title.ToLower().Trim() && c.Id == category.Id);

            if(isExist)
            {
                ModelState.AddModelError("Title", "Category with this name was Declared!");
                return View(category);
            }

            DbCategory.Title = category.Title;
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _appDbContext.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        #endregion
    }

}
