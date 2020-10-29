using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yenviethue.Data;
using Yenviethue.Models;
using Yenviethue.Models.ViewModels;
using Yenviethue.Service;
using Yenviethue.Utility;

namespace Yenviethue.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminUser + "," + SD.OrderManagerUser)]
    public class OrderManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private int PageSize = 5;

        public OrderManagerController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }



        [Authorize]
        public async Task<IActionResult> Index(int page = 1)
        {
            OrderListVM model = new OrderListVM()
            {
                OrderList = new List<OrderVM>()
            };

            List<Order> OrderHeaderList = await _db.Order.Include(o => o.ShippingInfo)
                                                    .Where(o => o.Status.Equals(SD.StatusSubmitted)
                                                    || o.Status.Equals(SD.StatusConfirmed)
                                                    || o.Status.Equals(SD.StatusInProcess)).ToListAsync();

            int skipTimes = (page - 1) * PageSize;
            for (int i = 0; i < OrderHeaderList.Count; i++)
            {
                if (i >= skipTimes)
                {
                    OrderVM individual = new OrderVM
                    {
                        OrderHeader = OrderHeaderList[i],
                        OrderProductList = await _db.OrderProduct.Where(o => o.OrderId == OrderHeaderList[i].Id).ToListAsync()
                    };
                    model.OrderList.Add(individual);
                }
            }

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.Order.Where(o => o.Status.Equals(SD.StatusSubmitted)
                                                    || o.Status.Equals(SD.StatusConfirmed)
                                                    || o.Status.Equals(SD.StatusInProcess)).CountAsync(),
                urlParam = "/Admin/OrderManager&page=:"
            };

            return View(model);
        }


        public async Task<IActionResult> SetStatusConfirmed(int OrderId)
        {
            var orderHeader = await _db.Order.FindAsync(OrderId);
            orderHeader.Status = SD.StatusConfirmed;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> SetStatusInProcess(int OrderId)
        {
            var orderHeader = await _db.Order.FindAsync(OrderId);
            orderHeader.Status = SD.StatusInProcess;
            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Yenviet Hue - Đơn hàng của bạn đang được vận chuyển giao hàng " + orderHeader.Id.ToString(), "Đơn hàng của bạn đã được xác nhận và đang trong quá trình vận chuyển giao hàng.");
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> SetStatusCompleted(int OrderId)
        {
            Order orderHeader = await _db.Order.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> SetStatusCancelled(int OrderId)
        {
            var orderHeader = await _db.Order.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();
            await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Yenviet Hue - Đơn hàng của bạn đã bị hủy " + orderHeader.Id.ToString(), "Đơn hàng của bạn đã bị hủy.");

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> AllOrder(int page = 1, string searchName = null, string searchPhone = null, string searchDate = null)
        {
            OrderListVM model = new OrderListVM()
            {
                OrderList = new List<OrderVM>()
            };

            #region Search Function
            StringBuilder param = new StringBuilder();
            param.Append("/Admin/OrderManager/AllOrder&page=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchDate=");
            if (searchDate != null)
            {
                param.Append(searchDate);
            }

            List<Order> OrderHeaderList = new List<Order>();

            if (searchName != null)
            {
                OrderHeaderList = await _db.Order.Include(o => o.ShippingInfo)
                                            .Where(o => o.ShippingInfo.Name.ToLower().Contains(searchName.ToLower()))
                                            .OrderByDescending(o => o.Id).ToListAsync();
            }
            else if (searchPhone != null)
            {
                OrderHeaderList = await _db.Order.Include(o => o.ShippingInfo)
                                            .Where(o => o.ShippingInfo.Phone.Contains(searchPhone))
                                            .OrderByDescending(o => o.Id).ToListAsync();
            }
            else if (searchDate != null)
            {
                if (DateTime.TryParse(searchDate, out DateTime date))
                {
                    OrderHeaderList = await _db.Order.Include(o => o.ShippingInfo)
                                                .Where(o => o.CreatedDate.Date.Equals(date))
                                                .OrderByDescending(o => o.Id).ToListAsync();
                }
            }
            else
            {
                OrderHeaderList = await _db.Order.Include(o => o.ShippingInfo).OrderByDescending(o => o.Id).ToListAsync();
            }
            #endregion

            int skipTimes = (page - 1) * PageSize;
            for (int i = 0; i < OrderHeaderList.Count; i++)
            {
                if (i >= skipTimes)
                {
                    OrderVM individual = new OrderVM
                    {
                        OrderHeader = OrderHeaderList[i],
                        OrderProductList = await _db.OrderProduct.Where(o => o.OrderId == OrderHeaderList[i].Id).ToListAsync()
                    };
                    model.OrderList.Add(individual);
                }
            }

            //var count = model.OrderList.Count;
            //model.OrderList = model.OrderList.OrderByDescending(p => p.OrderHeader.Id)
            //.Skip((page - 1) * PageSize)
            //.Take(PageSize).ToList();

            model.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItem = await _db.Order.CountAsync(),
                urlParam = param.ToString()
            };

            return View(model);
        }
    }
}