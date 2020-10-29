using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Utility;

namespace Spice.Areas.Customer.Controllers
{
    public class OrderController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;
        private int PageSize = 5;
        public OrderController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }



        public async Task<IActionResult> Confirm(int id)
        {
            var model = new OrderVM();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                model.OrderHeader = await _db.Order.Include(o => o.ShippingInfo).Where(o => o.Id == id && o.UserId == claim.Value).FirstOrDefaultAsync();
                model.OrderProductList = await _db.OrderProduct.Where(op => op.OrderId == id).ToListAsync();
            }
            else if (HttpContext.Session.GetInt32(SD.ssSuccessfulOrderId) == id)
            {
                model.OrderHeader = await _db.Order.Include(o => o.ShippingInfo).Where(o => o.Id == id).FirstOrDefaultAsync();
                model.OrderProductList = await _db.OrderProduct.Where(op => op.OrderId == id).ToListAsync();
            }

            return View(model);
        }

        [Authorize]
        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _db.Order.Where(m => m.Id == Id).FirstOrDefault().Status);
        }


        [Authorize]
        public async Task<IActionResult> OrderHistory(int page = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderListVM orderListVM = new OrderListVM()
            {
                OrderList = new List<OrderVM>()
            };



            List<Order> OrderHeaderList = await _db.Order.Include(o => o.ShippingInfo)
                                                        .Where(o => o.UserId == claim.Value)
                                                        .Skip((page - 1) * PageSize)
                                                        .Take(PageSize)
                                                        .ToListAsync();

            foreach (var order in OrderHeaderList)
            {
                OrderVM individual = new OrderVM
                {
                    OrderHeader = order,
                    OrderProductList = await _db.OrderProduct.Where(o => o.OrderId == order.Id).ToListAsync()
                };
                orderListVM.OrderList.Add(individual);
            }

            //var count = orderListVM.OrderList.Count;
            //orderListVM.OrderList = orderListVM.OrderList.OrderByDescending(p => p.OrderHeader.Id)
            //                     .Skip((page - 1) * PageSize)
            //                     .Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.Order.Where(o => o.UserId == claim.Value).CountAsync(),
                urlParam = "/Order/OrderHistory&page=:"
            };

            return View(orderListVM);
        }

        

        [Authorize]
        public async Task<IActionResult> GetOrderDetails(int Id)
        {
            OrderVM orderDetailsViewModel = new OrderVM()
            {
                OrderHeader = await _db.Order.Include(o => o.ShippingInfo).FirstOrDefaultAsync(m => m.Id == Id),
                OrderProductList = await _db.OrderProduct.Where(m => m.OrderId == Id).ToListAsync()
            };

            return PartialView("_IndividualOrderDetails", orderDetailsViewModel);
        }
    }
}