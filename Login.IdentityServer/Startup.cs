// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            services.AddAuthentication()
            .AddGoogle("Google", options =>
            {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    options.ClientId = "1032039370296-0a8p0eumo4pqet064m9g735beskfsfv1.apps.googleusercontent.com";
                    options.ClientSecret = "D2_moj6aTJ0cCIncyLa1sc5V";
            }); 

            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.WithOrigins("http://localhost:8080");
                    policy.AllowCredentials();
                });
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "http://localhost:8080/login.html";
                options.UserInteraction.ErrorUrl = "http://localhost:8080/error.html";
                options.UserInteraction.LogoutUrl = "http://localhost:8080/logout.html";

                if (!string.IsNullOrEmpty(Configuration["Issuer"]))
                {
                    options.IssuerUri = Configuration["Issuer"];
                }
            })
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetTestUsers());

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            var cors = new DefaultCorsPolicyService(new LoggerFactory().CreateLogger<DefaultCorsPolicyService>())
            {
                AllowedOrigins = { "http://localhost:8080"},
                AllowAll = true,
                
            };
            
            services.AddControllers();
            services.AddSingleton<ICorsPolicyService>(cors);
            services.AddTransient<IReturnUrlParser, ReturnUrlParser>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
