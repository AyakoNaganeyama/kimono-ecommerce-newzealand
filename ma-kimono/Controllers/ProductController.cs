using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ma_kimono.Models.DB;
using Newtonsoft.Json;
using X.PagedList;
using ma_kimono.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ma_kimono.Models;

/**
 * ============================================================================================================
 * Please Note: Admin and managers CRUD functionaly can be found here 
 * - Create product
 * - Read product data from database using get requests and queries
 * - Update product data, product information is spread between tables, product and orderItem
 * - Detele product data that is shared across tables, product and orderItem
 * 
 * Authentication:
 * - Customers can view all products, query products and view product details
 * - Only admins and managers can access CRUD functionality
 * ============================================================================================================
 */
namespace ma_kimono.Controllers
{
    public class ProductController : Controller
    {
        // 27/05 Ayako
        private readonly Fs2024s1Project01ProjectContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(Fs2024s1Project01ProjectContext context, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, int? page, string sortOrder, int? category)
        {
            // Find User (for discount) 
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user != null)
            {
                var Uid = user?.Id;
                // find cusotomer from user Id 12/06 Ayako
                if (!String.IsNullOrEmpty(Uid))
                {
                    var customer = await _context.Customers
                                             .Include(c => c.User)
                                             .FirstOrDefaultAsync(c => c.UserId == Uid);

                    //put in view bag
                    bool isMember = customer?.IsSubscribed ?? false;
                    ViewBag.IsMember = isMember;
                }
            }
            else
            {
                // if no user assign false
                ViewBag.IsMember = false;
            }

            ViewBag.CurrentSortOrder = sortOrder;
            // short hand if statment
            ViewBag.PriceSortParam = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            var pageNumber = page ?? 1;

            // get all products
            var products = from p in _context.Products select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                //Use LINQ for search
                products = products.Where(s => s.ProductName.Contains(searchString));
            }

            //Filtering logic 7/6 Ayako
            if (category != null)
            {
                products = products.Where(p => p.CategoryId == category);
            }

            //sorting Logic 7/6 Ayako
            switch (sortOrder)
            {
                case "price_desc":
                    products = products.OrderByDescending(s => s.ProductPrice);
                    break;
                default:
                    products = products.OrderBy(s => s.ProductPrice);
                    break;
            }

            return View(products.ToPagedList(pageNumber, 6));
        }

        
        public string IndexAJAX(string searchString)
        {
            string sql = "SELECT * FROM Product WHERE ProductName LIKE @p0";
            string wrapString = "%" + searchString + "%";
            List<Product> products = _context.Products.FromSqlRaw(sql, wrapString).ToList();
            string json = JsonConvert.SerializeObject(products);
            return json;
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            //ViewModel
            ProductDetail detail = new ProductDetail
            {
                Id = product.ProductId,
                name = product.ProductName,
                description = product.ProductDescription,
                price = product.ProductPrice,
                qty = product.ProductQty,
                material = product.ProductMaterial,
                size = product.Size,
                colour = product.Colour,
                img = product.ProductImg,
                brand = product.Brand?.BrandName,
                cate = product.Category?.CategoryName
            };

            return View(detail);
        }

        // GET: Product/Create
        [Authorize(Roles = "manager,admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductDescription,ProductPrice,ProductQty,CategoryId,ProductMaterial,BrandId,Size,Colour,ProductImg")] Product product, IFormFile ProductImg)
        {
            if (ModelState.IsValid)
            {
                if (ProductImg != null)
                {
                    // toggel category and set root manually
                    string folderPath = product.CategoryId == 1 ? "wwwroot/img/imgman" : "wwwroot/img/imgwoman";

                    // random name for file also set cap to make sure string isn't too long as length can reach 65 and will upset the database
                    string uniqueFileName = Guid.NewGuid().ToString().Substring(0,6) + "_" + ProductImg.FileName;
                    
                    string filePath = Path.Combine(folderPath, uniqueFileName).Replace("\\", "/");
                    
                    using (var fileStrem = new FileStream(filePath, FileMode.Create))
                    {
                        await ProductImg.CopyToAsync(fileStrem); // upload image 
                    }

                    // set image path, help set the root correctly taking hosting into account
                    product.ProductImg = filePath.Replace("wwwroot/", "~/"); 
                }

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductDescription,ProductPrice,ProductQty,CategoryId,ProductMaterial,BrandId,Size,Colour,ProductImg")] Product product, IFormFile ProductImg)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (ProductImg != null)
                {
                    // toggel category and set root manually
                    string folderPath = product.CategoryId == 1 ? "wwwroot/img/imgman" : "wwwroot/img/imgwoman";
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ProductImg.FileName;
                    
                    string filePath = Path.Combine(folderPath, uniqueFileName).Replace("\\", "/");

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProductImg.CopyToAsync(fileStream);
                    }
                    // set image path, help set the root correctly taking hosting into account
                    product.ProductImg = filePath.Replace("wwwroot/", "~/");
                }

                _context.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "BrandId", "BrandId", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'Fs2024s1Project01ProjectContext.Products'  is null.");
            }
            // get product and orderItem tables to make sure all product data deleted due to Fk
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                // get orderItems related to the product by id
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.ProductId == id)
                    .ToListAsync();

                // if there are orders with the product then delete it from orderItems
                if(orderItems != null)
                {
                    foreach(var item in orderItems)
                    {
                        _context.OrderItems.Remove(item);
                    }

                }

                // delete product
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}

