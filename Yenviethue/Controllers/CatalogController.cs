using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Utility;

namespace Yenviethue.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = SD.PageSize;

        public CatalogController(ApplicationDbContext db)
        {
            _db = db;
        }

        public CatalogVM ViewModel { get; set; }

        [Route("/danh-muc-san-pham")]
        public async Task<IActionResult> Index()
        {
            ViewModel = new CatalogVM()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                RepoSubCategoryList = new Dictionary<int, List<Models.SubCategory>>()
            };

            foreach (var category in ViewModel.CategoryList)
            {
                var subcList = await _db.SubCategories.Where(s=>s.CategoryId == category.Id).ToListAsync();
                ViewModel.RepoSubCategoryList.Add(category.Id, subcList);
            }

            return View(ViewModel);
        }


        [Route("/danh-muc/{id}")]
        public async Task<IActionResult> Category(int id)
        {
            ViewModel = new CatalogVM()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                RepoSubCategoryList = new Dictionary<int, List<Models.SubCategory>>()
            };

            foreach (var category in ViewModel.CategoryList)
            {
                var subcList = await _db.SubCategories.Where(s => s.CategoryId == category.Id).ToListAsync();
                ViewModel.RepoSubCategoryList.Add(category.Id, subcList);
            }

            ViewModel.CategoryCurrent = await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);

            if (ViewModel.CategoryCurrent != null)
            {
                ViewModel.ProductList = await _db.Products.Include(p=>p.SubCategory).Where(p => p.SubCategory.CategoryId == id).ToListAsync();

                return View(ViewModel);
            }

            return View(ViewModel);
        }


        [Route("/phu-muc/{id}")]
        public async Task<IActionResult> SubCategory(int id)
        {
            ViewModel = new CatalogVM()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                RepoSubCategoryList = new Dictionary<int, List<Models.SubCategory>>()
            };

            foreach (var category in ViewModel.CategoryList)
            {
                var subcList = await _db.SubCategories.Where(s => s.CategoryId == category.Id).ToListAsync();
                ViewModel.RepoSubCategoryList.Add(category.Id, subcList);
            }

            ViewModel.SubCategoryCurrent = await _db.SubCategories.Include(s => s.Category).SingleOrDefaultAsync(s => s.Id == id);
            ViewModel.CategoryCurrent = ViewModel.SubCategoryCurrent.Category;

            if (ViewModel.SubCategoryCurrent != null)
            {
                ViewModel.ProductList = await _db.Products.Where(p => p.SubCategoryId == id).ToListAsync();

                return View(ViewModel);
            }

            return NotFound();
        }


        [Route("/san-pham/{id}")]
        public async Task<IActionResult> Product(int id)
        {
            var productFromDb = await _db.Products.SingleOrDefaultAsync(p => p.Id == id);

            if (productFromDb == null) 
            {
                return NotFound();
            }

            return View(productFromDb);
        }


        public async Task<IActionResult> GetCategoryMenu(int id)
        {
            ViewModel = new CatalogVM()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                RepoSubCategoryList = new Dictionary<int, List<Models.SubCategory>>()
            };

            //foreach (var category in ViewModel.CategoryList)
            //{
            //    var subcList = await _db.SubCategories.Where(s => s.CategoryId == category.Id).ToListAsync();
            //    ViewModel.RepoSubCategoryList.Add(category.Id, subcList);
            //}

            //ViewModel.CategoryCurrent = await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);

            //if (ViewModel.CategoryCurrent != null)
            //{
            //    ViewModel.ProductList = await _db.Products.Include(p => p.SubCategory).Where(p => p.SubCategory.CategoryId == id).ToListAsync();

            //    return View(ViewModel);
            //}

            return PartialView("_CategoryMenu", ViewModel);
        }


        public async Task<IActionResult> Search(string search = "", int page = 1)
        {
            ProductVM model = new ProductVM
            {
                ProductList = await _db.Products.Where(p => p.Name.ToLower().Contains(search.ToLower()))
                                                .Skip((page - 1) * PageSize)
                                                .Take(PageSize)
                                                .ToListAsync()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Catalog/Search?search=");
            param.Append(search);
            param.Append("&page=:");

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.Products.Where(p => p.Name.ToLower().Contains(search.ToLower())).CountAsync(),
                urlParam = param.ToString()
            };

            return View(model);
        }
    }
}