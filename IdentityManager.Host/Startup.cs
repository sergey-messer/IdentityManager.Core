using IdentityManager.Host.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityManager.Host.Data;
using IdentityManager.Host.Models;
using IdentityManager.Host.Services;
using Messer.TaxiBooker.Api.Domain.IdentityManager;
using Microsoft.AspNetCore.Authorization;
using TzIdentityManager;
using TzIdentityManager.Configuration;
using TzIdentityManager.Core;

namespace IdentityManager.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, EmailSender>();
            
            services.AddMvc();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddInMemoryClients(Clients.Get())
                .AddAspNetIdentity<ApplicationUser>();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }


            app.UseIdentityManager(idmServices =>
            {
                idmServices.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                idmServices.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                idmServices.AddTransient
                    <IIdentityManagerService,
                        AspNetCoreIdentityManagerService<ApplicationUser, IdentityRole>>();

                idmServices.Configure<IdentityManagerOptions>(o =>
                {
                    o.Authority = "http://localhost:5001";
                    o.SecurityConfiguration = new HostSecurityConfiguration{
                        HostAuthenticationType = "Cookies",
                        AdditionalSignOutType = "oidc",
                        AdminRoleName = "admin",
                        RoleClaimType = "role"
                    };
                });

            },new IdentityManagerOptions()
            {
                Authority ="http://localhost:5001",
                SecurityConfiguration = new HostSecurityConfiguration{
                    HostAuthenticationType = "Cookies",
                    AdditionalSignOutType = "oidc",
                    RequireSsl = false,
                    BearerAuthenticationType="Bearer",
                    AdminRoleName = "admin",
                    RoleClaimType = "role",
                    AuthorizationPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(Constants.BearerAuthenticationType)
                        .RequireRole("admin")
                        .RequireAuthenticatedUser()
                        .Build()
                },
            });

            
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
