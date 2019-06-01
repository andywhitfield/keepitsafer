using System;
using System.Threading.Tasks;
using AspNet.Security.OpenId;
using KeepItSafer.Crypto.PasswordGenerator;
using KeepItSafer.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web
{
    public class Startup
    {
        private IHostingEnvironment hostingEnvironment;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            services
                .AddAuthentication(o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(o =>
                {
                    o.LoginPath = "/signin";
                    o.LogoutPath = "/signout";
                    o.Cookie.HttpOnly = true;
                    o.ExpireTimeSpan = TimeSpan.FromDays(150);
                })
                .AddOpenId("SmallId", "SmallId", o =>
                {
                    o.CallbackPath = "/signin-smallid";
                    o.Configuration = new OpenIdAuthenticationConfiguration
                    {
                        AuthenticationEndpoint = "https://smallid.nosuchblogger.com/openid/provider"
                    };
                });

            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            services.Configure<CookiePolicyOptions>(o =>
            {
                o.CheckConsentNeeded = context => false;
                o.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<SqliteDataContext>();

            // Add framework services.
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddSessionStateTempDataProvider();
            services.AddCors();
            services.AddDistributedMemoryCache();
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(5));
            services.AddSingleton<RazorTemplateEngine, KeepItSaferCompilationService>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddSingleton<WordDictionary>(serviceProvider =>
            {
                var dict = new WordDictionary();
                Task.Run(() => dict.LoadAsync());
                return dict;
            });
            services.AddTransient<RandomPasswordGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
