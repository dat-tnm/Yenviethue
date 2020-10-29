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
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Categories.ToListAsync());
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var files = HttpContext.Request.Form.Files;

            //_db has added category, so Id value should be generated in category.Id
            var categoryFromDb = await _db.Categories.FindAsync(category.Id);

            if (files.Count > 0)
            {
                //files has been uploaded
                string webRootPath = _webHostEnvironment.WebRootPath;
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, categoryFromDb.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(filesStream);
                }
                categoryFromDb.ImgSrc = @"\images\" + categoryFromDb.Id + extension;
            }
            else if (category.ImgSrc == null)
            {
                //if input(url img) was also null, so use default
                categoryFromDb.ImgSrc = @"~/images/default.png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            var categoryFromDb = await _db.Categories.FindAsync(category.Id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            //Work on image saving section

            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            //Delete the original file
            var upload = Path.Combine(webRootPath, "images");
            var imagePath = Path.Combine(webRootPath, categoryFromDb.ImgSrc.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            //Upload the new file
            if (files.Count > 0)
            {
                var extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload, categoryFromDb.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);
                }
                categoryFromDb.ImgSrc = @"\images\" + categoryFromDb.Id + extension;
            }
            else if (category.ImgSrc != null)
            {
                categoryFromDb.ImgSrc = category.ImgSrc;
            }

            _db.Update(categoryFromDb);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET - Delete
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.SingleOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;
                var imagePath = Path.Combine(webRootPath, category.ImgSrc.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}