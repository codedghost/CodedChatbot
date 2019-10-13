using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TwitchLib.Api;

namespace CoreCodedChatbot.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigService().GetConfig();

            services.AddOptions();
            services.AddMemoryCache();

            services.AddAuthentication(op =>
                {
                    op.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.ApiSecretSymmetricKey)),
                        ValidateIssuer = true,
                        ValidIssuer = config.ApiValidIssuer,
                        ValidateAudience = true,
                        ValidAudience = config.ApiValidAudience
                    };
                });

            services.AddMvc();

            var api = new TwitchAPI();
            api.Settings.AccessToken = config.ChatbotAccessToken;

            services.AddSingleton(api);
            services.AddSingleton<IChatbotContextFactory, ChatbotContextFactory>();
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IVipService, VipService>();
            services.AddSingleton<IGuessingGameService, GuessingGameService>();
            services.AddSingleton<IPlaylistService, PlaylistService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
