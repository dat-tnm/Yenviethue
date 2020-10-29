using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Utility;

namespace Yenviethue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminUser)]
    public class TagController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TagController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Tags.ToListAsync());
        }

        //API - GET
        public async Task<IActionResult> GetTagList()
        {
            List<Tag> tagList = await _db.Tags.ToListAsync();

            return Json(tagList);
        }


        //API - GET
        public async Task<IActionResult> GetTagsByProductId(int id)
        {
            List<Tag> tagList = await _db.ProductTags.Include(pt => pt.Tag)
                .Where(pt => pt.ProductId == id)
                .Select(pt => pt.Tag)
                .ToListAsync();

            return Json(tagList);
        }


        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (ModelState.IsValid)
            {
                tag.Name = tag.Name.ToUpper();

                _db.Tags.Add(tag);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _db.Tags.SingleOrDefaultAsync(c => c.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tag tag)
        {
            if (ModelState.IsValid)
            {
                tag.Name = tag.Name.ToUpper();

                _db.Update(tag);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _db.Tags.SingleOrDefaultAsync(c => c.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _db.Tags.SingleOrDefaultAsync(c => c.Id == id);

            if (tag != null)
            {
                _db.Tags.Remove(tag);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}