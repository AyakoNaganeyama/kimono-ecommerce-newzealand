using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ma_kimono.Models.DB;
using ma_kimono.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ma_kimono.Areas.Identity.Data;

/**
 * ============================================================================================================ 
 * Please Note: Admin and managers CRUD functionaly can be found here 
 * - Create users
 * - Read user data from database using get requests and queries
 * - Update user data, user information is shared between two tables Customer and User
 * - Detele user data that is shared across multiple tables such as order, orderItems, customer and user
 * 
 * Authentication:
 * - Customers can only access updating their subscription if they wish to unsubscribe
 * - Only admins and managers can access CRUD functionality
 * ============================================================================================================
 */

namespace ma_kimono.Controllers
{
    public class CustomerController : Controller
    {
        private readonly Fs2024s1Project01ProjectContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CustomerController(Fs2024s1Project01ProjectContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Index()
        {
            var fs2024s1Project01ProjectContext = _context.Customers.Include(c => c.User);
            return View(await fs2024s1Project01ProjectContext.ToListAsync());
        }

        // GET: Customer/Details/5
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer?.User == null)
            {
                return NotFound();
            }

            CustomerDetails d = new CustomerDetails()
            {
                customerID = customer.CustomerId,
                CustomerMobileNumber = customer.CustomerMobileNumber,
                CustomerAddress = customer.CustomerAddress,
                CustomerPaymentMethod = customer.CustomerPaymentMethod,
                IsSubscribed = customer.IsSubscribed,
                Id = customer.User.Id,  // Still use conditional access for User properties
                FirstName = customer.User.FirstName,
                LastName = customer.User.LastName,
                Email = customer.User.Email,
                PhoneNumber = customer.User.PhoneNumber,
            };

            return View(d);
        }

        // GET: Customer/Create
        [Authorize(Roles = "manager,admin")]
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Create([Bind("CustomerId,CustomerMobileNumber,CustomerAddress,CustomerPaymentMethod,UserId,IsSubscribed")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", customer.UserId);
            return View(customer);
        }

        // GET: Customer/Edit/5
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            
            var customer = await _context.Customers
                 .Include(c => c.User)
                 .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }
  
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerMobileNumber,CustomerAddress,CustomerPaymentMethod,UserId,IsSubscribed")] Customer customer, string FirstName, string LastName, string Phone)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    var UserI = new SelectList(_context.AspNetUsers, "Id", "Id", customer.UserId);
                    var user = await _context.AspNetUsers.FindAsync(customer.UserId);

                    if (user != null)
                    {
                        user.FirstName = FirstName;
                        user.LastName = LastName;
                        user.PhoneNumber = Phone;

                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                }
                return RedirectToAction(nameof(Index));
            }


            ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", customer.UserId);
            return View(customer);
        }

        // GET: Customer/Delete/5
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "manager,admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'Fs2024s1Project01ProjectContext.Customers'  is null.");
            }

           
            var customer = await _context.Customers.FindAsync(id);

            if (_context.AspNetUsers == null)
            {
                return Problem("Entity set 'Fs2024s1Project01ProjectContext.Customers'  is null.");
            }

            if (customer == null)
            {
                return NotFound();
            }

           
            var orders = await _context.Orders.Where(o =>  o.CustomerId == customer.CustomerId).ToListAsync();

            // loop through all order ids
            foreach(var order in orders)
            {
                // get all items related to the current order
                var items = await _context.OrderItems.Where(i => i.OrderId ==  order.OrderId).ToListAsync();    

                foreach(var item in items)
                {
                    // delete item from order items table
                    _context.OrderItems.Remove(item);
                }

                // remove the current order from the orders table
                _context.Orders.Remove(order);
            }

            // must clear these tables first before remove customer and user
            await _context.SaveChangesAsync();

            var user = await _context.AspNetUsers.FindAsync(customer.UserId);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            if (user != null)
            {
                _context.AspNetUsers.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangeSubscription()
        {
            return View();
        }

        // for customer to change their subscription
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeSubscription(bool Ismember) {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var Uid = user?.Id;

            var customer = await _context.Customers
                                           .Include(c => c.User)
                                           .FirstOrDefaultAsync(c => c.UserId == Uid);

            if (customer != null)
            {
                customer.IsSubscribed = Ismember;
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }

            return View();
        }

        private bool CustomerExists(int id)
        {
            return (_context.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }
    }
}
