using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yenviethue.Utility;

namespace Yenviethue.Extensions
{
    public class CreateGuestIdCookie
    {
        private readonly RequestDelegate _next;
        //private readonly IHttpContextAccessor _accessor;

        public CreateGuestIdCookie(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Cookies.ContainsKey(SD.cookieGuestId))
            {
                httpContext.Session.SetInt32(SD.ssCartCount, 0);
            }
            else if (httpContext.Session.GetInt32(SD.ssCartCount) == null
                    && !httpContext.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application"))
            {
                try
                {
                    var cartCount = int.Parse(httpContext.Request.Cookies[SD.cookieCartCount]);
                    httpContext.Session.SetInt32(SD.ssCartCount, cartCount);
                }
                catch (Exception)
                {
                    httpContext.Session.SetInt32(SD.ssCartCount, 0);
                }
            }

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CreateGuestIdCookieExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CreateGuestIdCookie>();
        }
    }
}
