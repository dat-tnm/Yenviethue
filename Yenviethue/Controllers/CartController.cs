using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Utility;

namespace Yenviethue.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public CartVM CartVM { get; set; }

        public CartController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }


        public async Task SavingCartCookieAsync(ShoppingCart inputCart)
        {
            //If browser doesn't have cookie guestId, it will be created in middleware pipeline
            string cookieGuestId = HttpContext.Request.Cookies[SD.cookieGuestId];

            if (HttpContext.Session.GetInt32(SD.ssCartCount) == 0)
            {
                if (await _db.Guest.FindAsync(cookieGuestId) == null)
                {
                    _db.Guest.Add(new Guest { Id = cookieGuestId, ExpirationDate = DateTime.Now.AddDays(SD.idleDaysCookies) });
                }
            }

            ShoppingCart cartFromDb = await _db.ShoppingCart.Where(sc => sc.GuestId == cookieGuestId
                                                        && sc.ProductId == inputCart.ProductId).FirstOrDefaultAsync();

            if (cartFromDb == null)
            {
                inputCart.GuestId = cookieGuestId;
                await _db.ShoppingCart.AddAsync(inputCart);
            }
            else
            {
                cartFromDb.Count = cartFromDb.Count + inputCart.Count;
            }
            await _db.SaveChangesAsync();

            //Do work with session
            var cartCount = _db.ShoppingCart.Where(c => c.GuestId == cookieGuestId).ToList().Count();
            HttpContext.Session.SetInt32(SD.ssCartCount, cartCount);

            //Save cartcount into cookie with guest user
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(SD.idleDaysCookies)
            };
            HttpContext.Response.Cookies.Append(SD.cookieCartCount, cartCount.ToString(), options);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(ShoppingCart inputCart)
        {

            inputCart.Id = 0;

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                inputCart.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _db.ShoppingCart.Where(sc => sc.ApplicationUserId == inputCart.ApplicationUserId
                                                    && sc.ProductId == inputCart.ProductId).FirstOrDefaultAsync();

                if (cartFromDb == null)
                {
                    await _db.ShoppingCart.AddAsync(inputCart);
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + inputCart.Count;
                }
                await _db.SaveChangesAsync();

                //Do work with session
                var cartCount = _db.ShoppingCart.Where(c => c.ApplicationUserId == inputCart.ApplicationUserId).Count();
                HttpContext.Session.SetInt32(SD.ssCartCount, cartCount);
            }
            else
            {
                //With user didn't login
                await SavingCartCookieAsync(inputCart);
            }


            string url = HttpContext.Request.Headers["Referer"];
            return Redirect(url);
        }


        public async Task<IActionResult> Index()
        {
            CartVM = new CartVM()
            {
                OrderHeader = new Order()
            };

            CartVM.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                CartVM.CartList = await _db.ShoppingCart.Where(sc => sc.ApplicationUserId == claim.Value).ToListAsync();
            }
            else
            {
                var guestId = Request.Cookies[SD.cookieGuestId];
                CartVM.CartList = await _db.ShoppingCart.Where(sc => sc.GuestId == guestId).ToListAsync();
            }

            foreach (var cart in CartVM.CartList)
            {
                cart.Product = await _db.Products.FirstOrDefaultAsync(m => m.Id == cart.ProductId);
                CartVM.OrderHeader.OrderTotal = CartVM.OrderHeader.OrderTotal + (cart.Product.Price * cart.Count);
            }
            CartVM.OrderHeader.OrderTotalOriginal = CartVM.OrderHeader.OrderTotal;


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                CartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == CartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                CartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, CartVM.OrderHeader.OrderTotalOriginal);
            }

            if (!Request.Cookies.ContainsKey(SD.cookieCartCount))
            {
                var cnt = CartVM.CartList.Count;
                CookieOptions options = new CookieOptions { Expires = DateTime.UtcNow.AddDays(SD.idleDaysCookies) };
                Response.Cookies.Append(SD.cookieCartCount, cnt.ToString());
                HttpContext.Session.SetInt32(SD.ssCartCount, cnt);
            }

            return View(CartVM);
        }


        #region PostMethodsOfIndex
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCoupon()
        {
            if (CartVM.OrderHeader.CouponCode == null)
            {
                CartVM.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, CartVM.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Plus(int cartId)
        {
            if (int.TryParse(Request.Form["amount-" + cartId.ToString()], out int amount))
            {
                var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
                if (cart.Count + amount > 0)
                {
                    cart.Count += amount;
                }
                else
                {
                    _db.ShoppingCart.Remove(cart);

                    var cnt = (int)HttpContext.Session.GetInt32(SD.ssCartCount);
                    HttpContext.Session.SetInt32(SD.ssCartCount, cnt - 1);

                    CookieOptions options = new CookieOptions { Expires = DateTime.UtcNow.Date.AddDays(SD.idleDaysCookies) };
                    Response.Cookies.Append(SD.cookieCartCount, (cnt - 1).ToString());
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Minus(int cartId)
        {
            if (int.TryParse(Request.Form["amount-" + cartId.ToString()], out int amount))
            {
                var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
                if (cart.Count - amount > 0)
                {
                    cart.Count -= amount;
                }
                else
                {
                    _db.ShoppingCart.Remove(cart);

                    var cnt = (int)HttpContext.Session.GetInt32(SD.ssCartCount);
                    HttpContext.Session.SetInt32(SD.ssCartCount, cnt - 1);

                    CookieOptions options = new CookieOptions { Expires = DateTime.UtcNow.Date.AddDays(SD.idleDaysCookies) };
                    Response.Cookies.Append(SD.cookieCartCount, (cnt - 1).ToString());
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = (int)HttpContext.Session.GetInt32(SD.ssCartCount);
            HttpContext.Session.SetInt32(SD.ssCartCount, cnt - 1);

            CookieOptions options = new CookieOptions { Expires = DateTime.UtcNow.Date.AddDays(SD.idleDaysCookies) };
            Response.Cookies.Append(SD.cookieCartCount, (cnt - 1).ToString());

            return RedirectToAction(nameof(Index));
        }
        #endregion


        public async Task<IActionResult> Summary()
        {

            CartVM = new CartVM()
            {
                OrderHeader = new Order()
            };

            CartVM.OrderHeader.OrderTotal = 0;
            CartVM.OrderHeader.ShippingFee = 50;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                CartVM.CartList = await _db.ShoppingCart.Where(sc => sc.ApplicationUserId == claim.Value).ToListAsync();
            }
            else
            {
                var guestId = Request.Cookies[SD.cookieGuestId];
                CartVM.CartList = await _db.ShoppingCart.Where(sc => sc.GuestId == guestId).ToListAsync();
            }

            foreach (var cart in CartVM.CartList)
            {
                cart.Product = await _db.Products.FirstOrDefaultAsync(m => m.Id == cart.ProductId);
                CartVM.OrderHeader.OrderTotal = CartVM.OrderHeader.OrderTotal + (cart.Product.Price * cart.Count);
            }
            CartVM.OrderHeader.OrderTotalOriginal = CartVM.OrderHeader.OrderTotal;


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                CartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == CartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                CartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, CartVM.OrderHeader.OrderTotalOriginal);
            }

            CartVM.OrderHeader.OrderTotal += CartVM.OrderHeader.ShippingFee;

            return View(CartVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                CartVM.CartList = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();
                CartVM.OrderHeader.UserId = claim.Value;
            }
            else if (Request.Cookies.ContainsKey(SD.cookieGuestId))
            {
                var guestid = Request.Cookies[SD.cookieGuestId];
                CartVM.CartList = await _db.ShoppingCart.Where(c => c.GuestId == guestid).ToListAsync();
            }

            CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            CartVM.OrderHeader.CreatedDate = DateTime.Now;
            CartVM.OrderHeader.Status = SD.PaymentStatusPending;

            _db.ShippingInfo.Add(CartVM.ShippingInfo);
            await _db.SaveChangesAsync();
            CartVM.OrderHeader.ShipmentId = CartVM.ShippingInfo.Id;

            _db.Order.Add(CartVM.OrderHeader);
            await _db.SaveChangesAsync();

            CartVM.OrderHeader.OrderTotalOriginal = 0;


            foreach (var item in CartVM.CartList)
            {
                item.Product = await _db.Products.FirstOrDefaultAsync(m => m.Id == item.ProductId);
                OrderProduct orderDetails = new OrderProduct
                {
                    ProductId = item.ProductId,
                    OrderId = CartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Amount = item.Count,
                    NameProduct = item.Product.Name
                };
                CartVM.OrderHeader.OrderTotalOriginal += orderDetails.Amount * orderDetails.Price;
                _db.OrderProduct.Add(orderDetails);
            }

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                CartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == CartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                CartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, CartVM.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                CartVM.OrderHeader.OrderTotal = CartVM.OrderHeader.OrderTotalOriginal;
            }
            CartVM.OrderHeader.CouponDiscount = CartVM.OrderHeader.OrderTotalOriginal - CartVM.OrderHeader.OrderTotal;
            CartVM.OrderHeader.OrderTotal += CartVM.OrderHeader.ShippingFee;

            _db.ShoppingCart.RemoveRange(CartVM.CartList);
            await _db.SaveChangesAsync();


            if (claim != null)
            {
                HttpContext.Session.SetInt32(SD.ssCartCount, 0);
                CartVM.ShippingInfo.UserId = claim.Value;
            }
            else
            {
                var guestFromDb = await _db.Guest.FindAsync(Request.Cookies[SD.cookieGuestId]);
                guestFromDb.ExpirationDate = DateTime.Now.AddDays(SD.idleDaysCookies);
                HttpContext.Response.Cookies.Append(SD.cookieCartCount, "0", new CookieOptions { Expires = DateTime.UtcNow.AddDays(SD.idleDaysCookies) });
                HttpContext.Session.SetInt32(SD.ssCartCount, 0);
                HttpContext.Session.SetInt32(SD.ssSuccessfulOrderId, CartVM.OrderHeader.Id);
            }


            #region Updating stripe transaction status
            var options = new Stripe.ChargeCreateOptions
            {
                Amount = Convert.ToInt32(CartVM.OrderHeader.OrderTotal * 100),
                Currency = "usd",
                Description = "Order ID : " + CartVM.OrderHeader.Id,
                Source = stripeToken

            };
            var service = new Stripe.ChargeService();
            Stripe.Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                CartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if (charge.Status.ToLower() == "succeeded")
            {
                //Email for successful order to Manager email
                await _emailSender.SendEmailAsync("tnmdat183@gmail.com", "Yenviet Hue - Order Created " + CartVM.OrderHeader.Id, "An order has been submitted !");

                CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                CartVM.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            #endregion

            await _db.SaveChangesAsync();
            return RedirectToAction("Confirm","Order", new { id = CartVM.OrderHeader.Id });
        }







    }
}