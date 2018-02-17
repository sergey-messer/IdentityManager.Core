using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using TzIdentityManager.Api.Models.AutoMapper;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();


            //services.AddIdentityManager(o =>
            //{
            //    var factory = new IdentityManagerServiceFactory();

            //    //var rand = new System.Random();
            //    //var users = Users.Get(rand.Next(5000, 20000));
            //    //var roles = Roles.Get(rand.Next(15));

            //    //factory.Register(new Registration<ICollection<InMemoryUser>>(users));
            //    //factory.Register(new Registration<ICollection<InMemoryRole>>(roles));
            //    //factory.IdentityManagerService = new Registration<IIdentityManagerService, AspNetCoreIdentityManagerService>();
            //    factory.IdentityManagerServiceDescriptor = ServiceDescriptor.Transient<IIdentityManagerService, AspNetCoreIdentityManagerService<ApplicationUser, IdentityRole>>();

            //    o.Factory = factory;
            //    o.SecurityConfiguration = new HostSecurityConfiguration();
            //});

            services.AddMvc();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddInMemoryClients(Clients.Get())
                .AddAspNetIdentity<ApplicationUser>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

                //idmServices.AddIdentityServer()
                //    .AddDeveloperSigningCredential()
                //    .AddInMemoryPersistedGrants()
                //    .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                //    .AddInMemoryApiResources(Resources.GetApiResources())
                //    .AddInMemoryClients(Clients.Get())
                //    .AddAspNetIdentity<ApplicationUser>();
            });




            //app.UseBranchWithServices("/idm",
            //    services => {
            //        services.AddDbContext<ApplicationDbContext>(options =>
            //            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //        services.AddIdentity<ApplicationUser, IdentityRole>()
            //            .AddEntityFrameworkStores<ApplicationDbContext>()
            //            .AddDefaultTokenProviders();

            //        services.AddMvc(opt =>
            //        {
            //            //opt.Filters.Add(new SampleGlobalActionFilter()); // an instance
            //        });
            //        services.AddIdentityManager(o =>
            //        {
            //            var factory = new IdentityManagerServiceFactory();

            //            //var rand = new System.Random();
            //            //var users = Users.Get(rand.Next(5000, 20000));
            //            //var roles = Roles.Get(rand.Next(15));

            //            //factory.Register(new Registration<ICollection<InMemoryUser>>(users));
            //            //factory.Register(new Registration<ICollection<InMemoryRole>>(roles));
            //            //factory.IdentityManagerService = new Registration<IIdentityManagerService, AspNetCoreIdentityManagerService>();
            //            factory.IdentityManagerServiceDescriptor = ServiceDescriptor.Transient<IIdentityManagerService, AspNetCoreIdentityManagerService<ApplicationUser, IdentityRole>>();

            //            o.Factory = factory;
            //            o.SecurityConfiguration = new HostSecurityConfiguration();
            //        });
            //    }
            //    ,
            //    appBuilder => {
            //            appBuilder.UseIdentityManager();
            //        }
            //    );


            //app.Map("/idm", idm =>
            //{
            //    idm.UseIdentityManager();
            //});

            //app.Map("/openid", id =>
            //{
            //    // use embedded identity server to issue tokens
            //    id.UseIdentityServer();
            //});

            app.UseStaticFiles();




            //app.UseAuthentication();// not needed, since UseIdentityServer adds the authentication middleware
            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}
