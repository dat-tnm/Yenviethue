using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int PageSize = SD.PageSize;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task AddTagsToProductAsync(int productId, bool isEditAction)
        {
            if (isEditAction)
            {
                var productTagFromDbs = await _db.ProductTags.Where(pt => pt.ProductId == productId).ToListAsync();
                _db.ProductTags.RemoveRange(productTagFromDbs);
                await _db.SaveChangesAsync();
            }

            var tagIds = HttpContext.Request.Form["taglist-checkbox"];

            if (tagIds.Count > 0)
            {

                foreach (var tagId in tagIds)
                {
                    _db.ProductTags.Add(new ProductTag() { ProductId = productId, TagId = Convert.ToInt32(tagId) });
                }
            }
            return;
        }


        public async Task<IActionResult> Index(int page = 1)
        {
            ProductVM productVM = new ProductVM()
            {
                ProductList = await _db.Products.Include(p => p.SubCategory).ThenInclude(s => s.Category)
                                        .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
                CategoryList = await _db.Categories.ToListAsync(),
                Product = new Product()
                {
                    SubCategory = new SubCategory()
                },
            };

            productVM.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.Products.CountAsync(),
                urlParam = "/Admin/Product&page=:"
            };

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProductVM modelSearch)
        {
            var productsFromDb = await _db.Products.Include(p => p.SubCategory).ToListAsync();

            modelSearch.Product.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"]);
            modelSearch.ProductList = productsFromDb.Where(s => s.IsSearchResult(modelSearch.Product) && s.SubCategory.IsSearchResult(modelSearch.Product.SubCategory)).ToList();
            modelSearch.CategoryList = await _db.Categories.ToListAsync();

            var tagId = HttpContext.Request.Form["TagId"];
            if (tagId.Count > 0)
            {
                var id = Convert.ToInt32(tagId[0]);

                if (id != 0)
                {
                    var productIds = await _db.ProductTags.Where(pt => pt.TagId == id).Select(pt => pt.ProductId).ToListAsync();
                    var newProductList = new List<Product>();

                    foreach (var p in modelSearch.ProductList)
                    {
                        if (productIds.Contains(p.Id))
                        {
                            newProductList.Add(p);
                        }
                    }

                    modelSearch.ProductList = newProductList;
                }
            }

            modelSearch.PagingInfo = new PagingInfo
            {
                CurrentPage = 1,
                ItemsPerPage = modelSearch.ProductList.Count,
                TotalItem = modelSearch.ProductList.Count,
                urlParam = "#"
            };

            return View(modelSearch);
        }


        //GET - CREATE
        public async Task<IActionResult> Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = await _db.Categories.ToListAsync()
            };

            return View(productVM);
        }


        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM productVM)
        {
            productVM.Product.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                productVM.CategoryList = await _db.Categories.ToListAsync();
                return View(productVM);
            }

            _db.Products.Add(productVM.Product);
            await _db.SaveChangesAsync();

            //_db has added productVM.Product, so Id value should be generated in productVM.Product.Id
            var productFromDb = await _db.Products.FindAsync(productVM.Product.Id);

            await AddTagsToProductAsync(productFromDb.Id, false);

            var files = HttpContext.Request.Form.Files;

            if (files.Count > 0)
            {
                //files has been uploaded
                string webRootPath = _webHostEnvironment.WebRootPath;
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, productFromDb.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(filesStream);
                }
                productFromDb.ImgSrc = @"\images\" + productFromDb.Id + extension;
            }
            else if (productVM.Product.ImgSrc == null)
            {
                //if input(url img) was also null, so use default
                productFromDb.ImgSrc = @"~/images/default.png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //GET - EDIT
        public async Task<IActionResult> Edit(int id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = await _db.Products.Include(p => p.SubCategory).ThenInclude(s => s.Category).SingleOrDefaultAsync(p => p.Id == id),
                CategoryList = await _db.Categories.ToListAsync(),
            };

            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM);
        }


        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductVM productVM)
        {

            if (!ModelState.IsValid)
            {
                productVM.CategoryList = await _db.Categories.ToListAsync();
                return View(productVM);
            }
            var productFromDb = await _db.Products.FindAsync(productVM.Product.Id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            await AddTagsToProductAsync(productFromDb.Id, true);

            //Work on image saving section

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            //Delete the original file
            var upload = Path.Combine(webRootPath, "images");
            var imagePath = Path.Combine(webRootPath, productFromDb.ImgSrc.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            //Upload the new file
            if (files.Count > 0)
            {
                var extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload, productFromDb.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);
                }
                productFromDb.ImgSrc = @"\images\" + productFromDb.Id + extension;
            }
            else if (productVM.Product.ImgSrc != null)
            {
                productFromDb.ImgSrc = productVM.Product.ImgSrc;
            }

            productFromDb.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            productFromDb.Name = productVM.Product.Name;
            productFromDb.Price = productVM.Product.Price;
            productFromDb.StockQuantity = productVM.Product.StockQuantity;
            productFromDb.DescriptionSummary = productVM.Product.DescriptionSummary;
            productFromDb.DescriptionDetails = productVM.Product.DescriptionDetails;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //GET - DELETE
        public async Task<IActionResult> Delete(int id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = await _db.Products.Include(p => p.SubCategory).ThenInclude(s => s.Category).SingleOrDefaultAsync(p => p.Id == id),
            };

            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM);
        }



        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.SingleOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = Path.Combine(webRootPath, product.ImgSrc.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}