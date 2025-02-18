using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ma_kimono.Models.DB;
using ma_kimono.VM;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace ma_kimono.Controllers
{
    [Authorize(Roles = "manager,admin")]
    public class SalesByBrandController : Controller
    {
        private readonly Fs2024s1Project01ProjectContext _context;
        private readonly string _salesByBrandSQL;

        public SalesByBrandController(Fs2024s1Project01ProjectContext context, IConfiguration configuration)
        {
            _context = context;
            _salesByBrandSQL = configuration["SQLQueries:SalesByBrandQuery"];
        }

        // GET: SalesByBrand
        public IActionResult Index()
        {
            var report = _context.SalesByBrandMV.FromSqlRaw(_salesByBrandSQL);
            return View(report);
        }

        public string GetBrand()
        {
            var report = _context.SalesByBrandMV.FromSqlRaw(_salesByBrandSQL);
            string json = JsonConvert.SerializeObject(report);
            return json;
        }

        // GET: SalesByBrand/Details/5
        public IActionResult Details(int? id)
        {
            var report = _context.SalesByBrandMV.FromSqlRaw(_salesByBrandSQL);
            return View(report.ToList());
        }

        public IActionResult Menu()
        {
            return View();
        }
    }
}
