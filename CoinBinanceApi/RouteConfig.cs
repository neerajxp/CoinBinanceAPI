using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoinBinanceApi
{
    public static class RouteConfig
    {
        public static void Include(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
            {
               // routes.MapHttpRoute(name: "swagger", routeTemplate: "", defaults: null, constraints: null, handler: new RedirectHandler((url => url.RequestUri.ToString()), "swagger"));

                // /
                routes.MapRoute(null, "", new
                {
                    controller = "Products",
                    action = "List",
                    category = "",
                    page = 1
                });

                // Page2
                routes.MapRoute(null,
                    "Page{page}",
                    new
                    {
                        controller = "Products",
                        action = "List",
                        category = ""
                    },
                    new { page = @"\d+" }
                );
            }
            );
        }
    }
}
