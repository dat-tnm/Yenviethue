using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yenviethue.Models;

namespace Yenviethue.Utility
{
    public static class SD
    {
        //role
        public const string AdminUser = "Admin";
        public const string OrderManagerUser = "OrderManager";
        public const string CustomerEndUser = "Customer";

        //session
        public const string ssCartCount = "ssCartCount";
        public const string ssCouponCode = "ssCouponCode";
        public const string ssSuccessfulOrderId = "ssSuccessfulOrderId";

        //cookie
        public const string cookieCartCount = "cart";
        public const string cookieGuestId = "guestId";
        public const int idleDaysCookies = 29;

        //order status
        public const string StatusSubmitted = "Submitted";
        public const string StatusConfirmed = "Confirmed, not yet shipped";
        public const string StatusInProcess = "Shipped";
        public const string StatusCompleted = "Delivered";
        public const string StatusCancelled = "Cancelled";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";

        //paging
        public const int PageSize = 10;


        public static double DiscountedPrice(Coupon couponFromDb, double OriginalOrderTotal)
        {
            if (couponFromDb == null || couponFromDb.MinimumAmount > OriginalOrderTotal)
            {
                return OriginalOrderTotal;
            }
            else
            {
                //everything is valid
                if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECouponType.VND)
                {
                    return Math.Round(OriginalOrderTotal - couponFromDb.Discount, 2);
                }
                else if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupon.ECouponType.Percent)
                {
                    return Math.Round(OriginalOrderTotal - (OriginalOrderTotal * couponFromDb.Discount / 100), 2);
                }
            }
            return OriginalOrderTotal;
        }

        public static string FormatCurrencyVND(double price)
        {
            return (price * 1000).ToString("C0").Substring(1) + "đ";
        }
    }
}
