using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CoinBinanceApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var contact = new OpenApiContact()
            {
                Name = "Neeraj Maurya",
                Email = "admin@defidecrypt.com",
                Url = new Uri("https://defidecrypt.com")
            };

            var license = new OpenApiLicense()
            {
                Name = "Defidecrypt License",
                Url = new Uri("https://defidecrypt.com")
            };

            var info = new OpenApiInfo()
            {
                Version = "v1",
                Title = "DefiDecrypt API",
                Description = "DefiDecrypt APIs for Cryptocurrency",
                TermsOfService = new Uri("https://defidecrypt.com"),
                Contact = contact,
                License = license
            };

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", info);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //RouteConfig.Include(app);
            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(
                     x => x.WithOrigins("https://defidecrypt.com")
                     .AllowAnyMethod()
                     .AllowAnyOrigin()
                     .AllowAnyHeader()
                    );

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json","MyAPI");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
