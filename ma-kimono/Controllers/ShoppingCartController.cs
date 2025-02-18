using ma_kimono.Areas.Identity.Data;
using ma_kimono.Models.DB;
using ma_kimono.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using X.PagedList.Mvc.Core;



namespace ma_kimono.Controllers
{
    
    public class ShoppingCartController : Controller
    {
        private readonly Fs2024s1Project01ProjectContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        //static shopping cart list
        public static List<OrderItem> items = new List<OrderItem>();
        public static string userId;

        public ShoppingCartController(Fs2024s1Project01ProjectContext context, UserManager<AppUser> userManager, SignInManager<AppUser> SignIn)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = SignIn;
        }

        [Authorize]
        public async Task<IActionResult> AddItem(int productId)
        {
            //get currently logged in user's id
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var Uid = user?.Id;

            // if logged in user does not match previous logged in user, the item is deletd 
            if (Uid != userId)
            {
                items.Clear();
            }

            // assign current user to static use id 
            userId = Uid;

            // find customer by UserId
            var customer = await _context.Customers
                                             .Include(c => c.User)
                                             .FirstOrDefaultAsync(c => c.UserId == userId);

            //put in view bag if it's memeber
            bool isMember = customer?.IsSubscribed ?? false;

            ViewBag.IsMember = isMember;

            // find product by id
            var product = await _context.Products.FirstAsync(p => p.ProductId == productId);

            OrderItem orderItem = new OrderItem();

            var itemFound = items.FirstOrDefault(item => item.ProductId == productId);

            if (itemFound != null)
            {

                itemFound.Qty++;
                // short hand if statement, match price with quanity and subscription
                itemFound.Subtotal = isMember ? product?.ProductPrice * itemFound.Qty * 0.8m : product?.ProductPrice * itemFound.Qty;

                return RedirectToAction("ViewOrderItems");
            }

            // fallback, new item added to cart
            orderItem.ProductId = productId;
            orderItem.Product = product;
            orderItem.Qty = 1;
            // shorthand if statment check if memeber set price accordingly
            orderItem.Subtotal = isMember ? product.ProductPrice * 0.8m : product.ProductPrice;

            items.Add(orderItem);

            return RedirectToAction("ViewOrderItems");
        }

        // Testing if oerder is added
        public IActionResult ShowAlert(string message)
        {
            ViewBag.AlertMessage = message;
            return View();
        }


        public async Task<IActionResult> ViewOrderItems()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var Uid = user?.Id;

            if (Uid != userId)
            {
                items.Clear();
            }

            userId = Uid;

            // find customer by UserId
            var customer = await _context.Customers
                                             .Include(c => c.User)
                                             .FirstOrDefaultAsync(c => c.UserId == userId);

            //put in view bag if it's memeber
            bool isMember = customer?.IsSubscribed ?? false;

            if (user != null && customer != null)
            {
                ViewBag.Member = isMember;
            }
            else
            {
                ViewBag.Member = false;
            }

            ViewBag.id = userId;

            return View(items);
        }


        public async Task<IActionResult> OrderComfirm(string UID)
        {
            //find customer by UserID
            if (items.Count > 0)
            {
                var customer = await _context.Customers
                                         .Include(c => c.User)
                                         .FirstOrDefaultAsync(c => c.UserId == UID);
                decimal total = 0;
                
                foreach (var i in items)
                {
                    total += i.Subtotal ?? 0;
                }
                
                Order order = new Order()
                {
                    OrderDate = DateTime.Now,
                    CustomerId = customer.CustomerId,
                    TotalAmount = total,
                };

                // add to database for order
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                //add to database for orderitem 
                foreach (var item in items)
                {
                    item.OrderId = order.OrderId;
                    // product was only needed to to display name so need to set back to null
                    item.Product = null; 
                    _context.OrderItems.Add(item);
                }
                await _context.SaveChangesAsync();

                //create Total Order Information 
                var user = await _userManager.GetUserAsync(HttpContext.User);
                OrderComfirmVM confirm = new OrderComfirmVM()
                {
                    FirstName = user.FirstName,
                    Address = customer.CustomerAddress,
                    total = order.TotalAmount,
                };

                //clear list 
                items.Clear();

                return View(confirm);
            }
            else
            {
                ViewBag.nothing = "No order";

                return RedirectToAction("OrderConfirmation");
            }
        }

        public IActionResult OrderConfirmation(string message)
        {
            ViewBag.Confirm = message;
            return View();
        }

        public IActionResult DeleteItem(int productId)
        {
            var item = items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                items.Remove(item);
            }
            
            return RedirectToAction("ViewOrderItems");
        }

        public string getItems()
        {
            return JsonConvert.SerializeObject(items, Formatting.Indented);
        }
    }
}
