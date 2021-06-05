using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoinBinanceApi.Common;
using CoinBinanceApi.DBContext;
using CoinBinanceApi.WorkerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
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
        public object ConnectionStringGetter { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        
            //Add Controllers
            services.AddControllers();

            //Add Background Services, Worker Processes
             AddBackgroundWorkers(services);

            //Allow Cors
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    // .AllowCredentials()
                    .Build();
                });
            });
             
             services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
 
            //Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "DeFiDecrypt API",
                    Description = "DefiDecrypt APIs for Cryptocurrency",
                    TermsOfService = new Uri("https://defidecrypt.com"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Admin",
                        Email = "admin@defidecrypt.com",
                        Url = new Uri("https://defidecrypt.com")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "DefiDecrypt License",
                        Url = new Uri("https://defidecrypt.com")
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer' [SPACE] and then token in the text input below",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference= new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            },
                            Scheme="oauth2",
                            Name="Bearer",
                            In=ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            //Authorization and Authentication
            app.UseAuthorization();
            //app.UseAuthentication();
 
            app.UseCors(
                     x => x.WithOrigins("https://defidecrypt.com")
                     .AllowAnyMethod()
                     .AllowAnyOrigin()
                     .AllowAnyHeader()
                    );


            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "DefiDecrypt API";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoinBinance API");
                c.RoutePrefix = string.Empty;   //default to swagger UI on startup
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void AddBackgroundWorkers(IServiceCollection services)
        {
            //Add the Gates Rates Service
            services.AddHostedService<GatesRatesService>();
            services.AddHostedService<CoinGeckoRatesService>();

            //var connectionString = Configuration.GetSection("SQLConnection").Value;
            //var connectionString = "test";
           // Global.constrGate= connectionString;
        }
    }
}
