using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Utility;

namespace Yenviethue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminUser)]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CouponController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _db.Coupons.ToListAsync();

            return View(coupons);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (!ModelState.IsValid)
            {
                return View(coupon);
            }

            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();

            var files = HttpContext.Request.Form.Files;

            var couponFromDb = await _db.Coupons.FindAsync(coupon.Id);
            //_db has added coupon, so Id value should be generated in coupon.Id

            if (files.Count > 0)
            {
                //files has been uploaded
                string webRootPath = _webHostEnvironment.WebRootPath;
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, couponFromDb.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(filesStream);
                }
                couponFromDb.ImgSrc = @"\images\" + couponFromDb.Id + extension;
            }
            else if (coupon.ImgSrc == null)
            {
                //if input(url img) was also null, so use default
                couponFromDb.ImgSrc = @"\images\default.png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _db.Coupons.SingleOrDefaultAsync(c => c.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (!ModelState.IsValid)
            {
                return View(coupon);
            }
            var couponFromDb = await _db.Coupons.FindAsync(coupon.Id);

            if (couponFromDb == null)
            {
                return NotFound();
            }

            //Work on image saving section

            var files = HttpContext.Request.Form.Files;

            if (files.Count > 0)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                var upload = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);
                //Delete the original file
                var imagePath = Path.Combine(webRootPath, couponFromDb.ImgSrc.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //Upload the new file
                using (var fileStream = new FileStream(Path.Combine(upload, couponFromDb.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);
                }
                couponFromDb.ImgSrc = @"\images\" + couponFromDb.Id + extension;
            }
            else if (coupon.ImgSrc != null)
            {
                couponFromDb.ImgSrc = coupon.ImgSrc;
            }

            couponFromDb.Name = coupon.Name;
            couponFromDb.MinimumAmount = coupon.MinimumAmount;
            couponFromDb.Discount = coupon.Discount;
            couponFromDb.CouponType = coupon.CouponType;
            couponFromDb.IsActive = coupon.IsActive;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _db.Coupons.SingleOrDefaultAsync(c => c.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coupon = await _db.Coupons.SingleOrDefaultAsync(c => c.Id == id);

            if (coupon != null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = Path.Combine(webRootPath, coupon.ImgSrc.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _db.Coupons.Remove(coupon);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}