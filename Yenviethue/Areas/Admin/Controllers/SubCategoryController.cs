using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Extensions;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Utility;

namespace Yenviethue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminUser)]
    public class SubCategoryController : Controller
    {

        private readonly ApplicationDbContext _db;
        private int PageSize = SD.PageSize;

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [TempData]
        public string StatusMessage { get; set; }


        public async Task<IActionResult> Index(int page = 1)
        {
            SubCategoryVM model = new SubCategoryVM()
            {
                SubCategoryList = await _db.SubCategories.Include(s => s.Category)
                                                        .Skip((page - 1) * PageSize)
                                                        .Take(PageSize)
                                                        .ToListAsync(),

                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = new SubCategory()
            };

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.SubCategories.CountAsync(),
                urlParam = "/Admin/SubCategory&page=:"
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SubCategoryVM modelSearch)
        {
            var subCategoriesFromDb = await _db.SubCategories.Include(s => s.Category).ToListAsync();
            modelSearch.SubCategoryList = subCategoriesFromDb.Where(s => s.IsSearchResult(modelSearch.SubCategory)).ToList();
            modelSearch.CategoryList = await _db.Categories.ToListAsync();

            modelSearch.PagingInfo = new PagingInfo
            {
                CurrentPage = 1,
                ItemsPerPage = modelSearch.SubCategoryList.Count,
                TotalItem = modelSearch.SubCategoryList.Count,
                urlParam = "#"
            };

            return View(modelSearch);
        }

        //GET - CREATE
        public async Task<IActionResult> Create()
        {
            SubCategoryVM model = new SubCategoryVM()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory(),
            };

            return View(model);
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var existedSubCategory = await _db.SubCategories.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).FirstOrDefaultAsync();

                if (existedSubCategory != null)
                {
                    //Error
                    StatusMessage = "Error: this SubCategory name exists under " + existedSubCategory.Category.Name + " Category. Please use another name.";
                }
                else
                {
                    _db.SubCategories.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryVM modelVM = new SubCategoryVM()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                StatusMessage = StatusMessage
            };

            return View(modelVM);
        }

        //API - GetSubCategory
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = await (from subCategory in _db.SubCategories
                                                     where subCategory.CategoryId == id
                                                     select subCategory).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var subCategory = await _db.SubCategories.Include(s => s.Category).SingleOrDefaultAsync(s => s.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryVM modelVM = new SubCategoryVM()
            {
                SubCategory = subCategory,
            };

            return View(modelVM);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var existedSubCategory = await _db.SubCategories.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).FirstOrDefaultAsync();

                if (existedSubCategory != null && existedSubCategory.Id != id)
                {
                    //Error
                    StatusMessage = "Error: this SubCategory name exists under " + existedSubCategory.Category.Name + " Category. Please use another name.";
                    model.SubCategory.Category = existedSubCategory.Category;
                }
                else
                {
                    var subCategoryFromDb = await _db.SubCategories.FindAsync(id);
                    subCategoryFromDb.Name = model.SubCategory.Name;
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryVM modelVM = new SubCategoryVM()
            {
                SubCategory = model.SubCategory,
                StatusMessage = StatusMessage,
            };

            return View(modelVM);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _db.SubCategories.Include(s => s.Category).SingleOrDefaultAsync(s => s.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryVM modelVM = new SubCategoryVM()
            {
                SubCategory = subCategory,
            };

            return View(modelVM);
        }

        //POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _db.SubCategories.SingleOrDefaultAsync(s => s.Id == id);

            if (subCategory != null)
            {
                _db.SubCategories.Remove(subCategory);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}