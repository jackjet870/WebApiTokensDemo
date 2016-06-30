﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace TokenCalc
{
    public class Startup
    {
        

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
 // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        //Our secred key which MUST be stored separately
        private static readonly string secretKey = "veryverysecretkey";



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var logger = loggerFactory.CreateLogger("Tokens");


            app.UseStaticFiles();

            //Setting up our "token dispenser"
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var options = new TokenProviderOptions
            {
                Audience = "ExampleAudience",
                Issuer = "ExampleIssuer",
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(options));


            //Setting our tokens validator
            var tokenparams = new TokenValidationParameters();
            tokenparams.IssuerSigningKey = signingKey;
            tokenparams.ValidIssuer = "ExampleIssuer";


            var opts = new JwtBearerOptions()
            {
                TokenValidationParameters = tokenparams,
                //token will be stored even after restart actually
                SaveToken = true,
                AutomaticAuthenticate = true,
                Audience = "ExampleAudience",
                Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        logger.LogError("Authentication failed.", context.Exception);
                        return Task.FromResult(0);
                    },
                    OnTokenValidated = context =>
                    {
                        //not identified kind of magic goes here
                        var claimsIdentity = context.Ticket.Principal.Identity as ClaimsIdentity;
                        claimsIdentity.AddClaim(new Claim("id_token",
                            context.Request.Headers["Authorization"][0].Substring(context.Ticket.AuthenticationScheme.Length + 1)));

                        // OPTIONAL: you can read/modify the claims that are populated based on the JWT
                        // claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, claimsIdentity.FindFirst("name").Value));
                        return Task.FromResult(0);
                    }
                }
            };

            //Adding a jwt service so we can use it like 
            //[Authorize(ActiveAuthenticationSchemes = "Bearer")]
            app.UseJwtBearerAuthentication(opts);
            

            app.UseMvc();
            
        }
    }
}
