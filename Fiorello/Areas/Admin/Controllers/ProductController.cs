using Fiorello.Areas.Admin.Helpers;
using Fiorello.Areas.Admin.ViewModels;
using Fiorello.Areas.Admin.ViewModels.Product;
using Fiorello.Areas.ViewModels.Product;
using Fiorello.DAL;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(AppDbContext appDbContext, IFileService fileService, IWebHostEnvironment webHostEnvironment)
        {
            _appDbContext = appDbContext;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ProductIndexViewModel
            {

                Products = await _appDbContext.Products.Include(x=>x.Category).ToListAsync()
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductCreateViewModel
            {
                Categories = await _appDbContext.Categories.Select(p => new SelectListItem
                {
                    Text = p.Title,
                    Value = p.Id.ToString()
                })
               .ToListAsync()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            model.Categories = await _appDbContext.Categories.Select(p => new SelectListItem
            {
                Text = p.Title,
                Value = p.Id.ToString()
            })
                .ToListAsync();
            if (!ModelState.IsValid) return View(model);
            var category = await _appDbContext.Categories.FindAsync(model.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "This category isnt declared");
                return View(model);
            }
            bool IsExist = await _appDbContext.Products
                .AnyAsync(p => p.Name.ToLower().Trim() == model.Name.ToLower().Trim());
            if (IsExist)
            {
                ModelState.AddModelError("Title", "This category inst declared");
                return View(model);
            }
            if (!_fileService.IsImage(model.MainPhotoName))
            {
                ModelState.AddModelError("MainPhotoName", "File must be img formatt");
                return View(model);

            }
            if (!_fileService.CheckSize(model.MainPhotoName, 500))
            {
                ModelState.AddModelError("MainPhoto", "fILE SIZE IS MOREN THAN REQUESTED");
                return View(model);
            }

            bool hasError = false;
            foreach (var photo in model.ProductPhotos)
            {
                if (!_fileService.IsImage(photo))
                {
                    ModelState.AddModelError("ProductPhotos", $"{photo.FileName} fILE MUST BE IMG FORMAT");
                    hasError = true;

                }
                else if (!_fileService.CheckSize(photo, 300))
                {
                    ModelState.AddModelError("ProductPhotos", $"{photo.FileName}DOWNLOAD fILE SIZE MUST BE LESS THAN 500KB");
                    hasError = true;

                }

            }

            if (hasError) { return View(model); }

            var product = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    Description = model.Description,
                    Quantity = model.Quantity,
                    Weight = model.Weight,
                    Dimenesion = model.Dimenesion,
                    CategoryId = model.CategoryId,
                    Status = model.Status,
                    MainPhotoName = await _fileService.UploadAsync(model.MainPhotoName, _webHostEnvironment.WebRootPath)
                };
                await _appDbContext.Products.AddAsync(product);
                await _appDbContext.SaveChangesAsync();
                return RedirectToAction("index");

            }
        }
    }

