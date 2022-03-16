using LessonMigrations.Data;
using LessonMigrations.Models;
using LessonMigrations.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LessonMigrations.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.ProductCount =  _context.Products.Where(p => p.İsDeleted == false).Count();
            List<Product> products = await _context.Products.Where(m => m.İsDeleted == false)
                  .Include(m => m.Category)
                  .Include(m => m.ProductImages)
                  .OrderByDescending(m => m.Id)
                  .Take(8)
                  .ToListAsync();
            return View(products);
        }
        public async  Task<IActionResult> LoadMore(int  skip)
        {
            List<Product> products = await _context.Products.Where(m => m.İsDeleted == false)
                 .Include(m => m.Category)
                 .Include(m => m.ProductImages)
                 .OrderByDescending(m => m.Id)
                 .Skip(skip)
                 .Take(4)
                 .ToListAsync();
            return PartialView("_ProductsPartial", products);
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id is null) return NotFound();
            Product dbproduct = await _context.Products.FindAsync(id);
            if (dbproduct == null) return BadRequest();
            List<BasketVM> basket;
            if (Request.Cookies["basket"] != null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);
            }
            else
            {
                basket = new List<BasketVM>();
            }
            var existProduct = basket.Find(m => m.Id == dbproduct.Id);
            if (existProduct == null)
            {
                basket.Add(new BasketVM
                {
                    Id = dbproduct.Id,
                    Count = 1
                });
            }
            else
            {
                existProduct.Count++;
            }
          
            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));
            return RedirectToAction("Index", "Home");
        }
        public IActionResult test()
        {
            var cookie = Request.Cookies["basket"];
            return Json(JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]));
        }
    }
}
