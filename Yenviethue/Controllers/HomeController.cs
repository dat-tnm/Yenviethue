using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Utility;

namespace Yenviethue.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = SD.PageSize;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IEnumerable<Product>> GetProductByTagAsync(string tagName)
        {
            var products = await _db.ProductTags.Include(pt => pt.Tag)
                .Where(pt => pt.Tag.Name.Equals(tagName))
                .Include(pt => pt.Product)
                .Select(pt => pt.Product)
                .ToListAsync();

            return products;
        }


        public async Task<IActionResult> Index()
        {
            var top4HotProducts = await _db.ProductTags.Include(pt => pt.Tag)
                .Where(pt => pt.Tag.Name.Equals("HOT"))
                .Include(pt => pt.Product)
                .Select(pt => pt.Product).Take(4)
                .ToListAsync();

            var top4NewProducts = await _db.ProductTags.Include(pt => pt.Tag)
                .Where(pt => pt.Tag.Name.Equals("NEW"))
                .Include(pt => pt.Product)
                .Select(pt => pt.Product).Take(4)
                .ToListAsync();

            HomeVM model = new HomeVM()
            {
                ListTop4HotProduct = top4HotProducts,
                ListTop4NewProduct = top4NewProducts
            };

            return View(model);
        }

        [Route("/san-pham-hot")]
        public async Task<IActionResult> HotProducts(int page = 1)
        {
            var products = await GetProductByTagAsync("HOT");
            ProductVM model = new ProductVM
            {
                ProductList = products.Skip((page - 1) * PageSize).Take(PageSize).ToList()
            };


            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = products.Count(),
                urlParam = "/san-pham-hot&page=:"
            };

            return View(model);
        }

        [Route("/san-pham-moi")]
        public async Task<IActionResult> NewProducts(int page = 1)
        {
            var products = await GetProductByTagAsync("NEW");
            ProductVM model = new ProductVM
            {
                ProductList = products.Skip((page - 1) * PageSize).Take(PageSize).ToList()
            };


            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = products.Count(),
                urlParam = "/san-pham-moi&page=:"
            };

            return View(model);
        }

        






            public IActionResult About()
        {
            return View();
        }

        public IActionResult Promotion()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
