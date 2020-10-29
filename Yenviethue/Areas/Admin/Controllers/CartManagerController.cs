using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Utility;

namespace Yenviethue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminUser)]
    public class CartManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = SD.PageSize;

        public CartManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var model = new CartManagerVM
            {
                CartList = await _db.ShoppingCart.Skip((page - 1) * PageSize).Take(PageSize).OrderBy(sp => sp.GuestId).ToListAsync()
            };

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.ShoppingCart.CountAsync(),
                urlParam = "/Admin/CartManager&page=:"
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOutDateCarts()
        {
            var outDateGuestIds = await _db.Guest.Where(g => g.ExpirationDate.Date.CompareTo(DateTime.Now.Date) >= 0).Select(g => g.Id).ToListAsync();

            var carts = await _db.ShoppingCart.Where(sc => outDateGuestIds.Contains(sc.GuestId)).ToListAsync();

            _db.ShoppingCart.RemoveRange(carts);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllCart()
        {
            var carts = await _db.ShoppingCart.ToListAsync();

            _db.ShoppingCart.RemoveRange(carts);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Guest(int page = 1)
        {
            var model = new CartManagerVM
            {
                GuestList = await _db.Guest.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync()
            };

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.Guest.CountAsync(),
                urlParam = "/Admin/CartManager/Guest&page=:"
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllGuestCart()
        {
            var carts = await _db.ShoppingCart.Where(sp => sp.GuestId != null).ToListAsync();
            _db.ShoppingCart.RemoveRange(carts);

            var guests = await _db.Guest.ToListAsync();
            _db.Guest.RemoveRange(guests);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}