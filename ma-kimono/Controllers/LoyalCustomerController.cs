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
    public class LoyalCustomerController : Controller
    {
        private readonly Fs2024s1Project01ProjectContext _context;
        private readonly string _loyalCustomerSQL;
        public LoyalCustomerController(Fs2024s1Project01ProjectContext context, IConfiguration configuration)
        {
            _context = context;
            _loyalCustomerSQL = configuration["SQLQueries:LoyalCustomerQuery"];
        }

        // GET: LoyalCustomer
        public IActionResult Index()
        {


            var report = _context.LoyalCustomers.FromSqlRaw(_loyalCustomerSQL);
            return View(report);
        }


        // GET: LoyalCustomer/Details/5
        public IActionResult Details(int? id)
        {


            var report = _context.LoyalCustomers.FromSqlRaw(_loyalCustomerSQL);
            return View(report.ToList());
        }

        public string GetCustomer()
        {


            var report = _context.LoyalCustomers.FromSqlRaw(_loyalCustomerSQL);
            string json = JsonConvert.SerializeObject(report);
            return json;
        }
    }
}
