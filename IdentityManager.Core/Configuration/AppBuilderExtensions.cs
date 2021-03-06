﻿using System;
using System.Security.Claims;
using AutoMapper;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using TzIdentityManager.Api.Models.AutoMapper;

namespace TzIdentityManager.Configuration
{
    public static class AppBuilderExtensions
    {


        //public static IServiceCollection AddIdentityManager(this IServiceCollection services, Action<IdentityManagerOptions> configureOptions)
        //{
        //    if (services == null)
        //        throw new ArgumentNullException(nameof(services));
        //    if (configureOptions == null)
        //        throw new ArgumentNullException(nameof(configureOptions));

        //    services.Configure<IdentityManagerOptions>(configureOptions);
        //    //var identityManagerOptions = new IdentityManagerOptions();
        //    //configureOptions.Invoke(identityManagerOptions);

        //    ////services.TryAdd(identityManagerOptions.Factory.IdentityManagerServiceDescriptor);
        //    //foreach (var registration in identityManagerOptions.Factory.Registrations)
        //    //{
        //    //    services.TryAdd(registration);
        //    //}

        //    services.AddAutoMapper(typeof(IdentityModelProfile));

        //    return services;
        //}

        public static void UseIdentityManager(this IApplicationBuilder appBuilder, Action<IServiceCollection> registerServices, IdentityManagerOptions identityManagerOptions)
        {
            if (appBuilder == null) throw new ArgumentNullException(nameof(appBuilder));
            if (identityManagerOptions == null) throw new ArgumentNullException(nameof(identityManagerOptions));

            
            //var identityManagerOptions = new IdentityManagerOptions();
            //configureOptions.Invoke(identityManagerOptions);
            //var options = appBuilder.ApplicationServices.GetService<IOptions<IdentityManagerOptions>>();

            //if (!string.IsNullOrEmpty(options.Value.Authority))
            //{
            //    appBuilder.UseBranchWithServices("/ids", null, null);
            //}

            Action<IServiceCollection> branchServices = idmServices =>
            {
                registerServices(idmServices);

                //idmServices.Configure<IdentityManagerOptions>(o=>
                //{
                //    o= identityManagerOptions;
                //});
                
                idmServices.AddAutoMapper(typeof(IdentityModelProfile));

                idmServices.AddMvc(opt =>
                {
                    if (identityManagerOptions.SecurityConfiguration.RequireSsl)
                    {
                        opt.Filters.Add(new RequireHttpsAttribute());
                    }

                    opt.Filters.Add(new AuthorizeFilter(identityManagerOptions.SecurityConfiguration.AuthorizationPolicy));
                });



                idmServices.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(option =>
                    {
                        option.Authority = identityManagerOptions.SecurityConfiguration.Authority;// "http://localhost:5001";
                        option.RoleClaimType = "role";
                        option.RequireHttpsMetadata = false;
                        option.ApiName = "api1";
                    });

            };

            appBuilder.UseBranchWithServices("/idm",branchServices,
                app => {

                    //app.Use(async (ctx, next) =>
                    //{
                    //    if (!ctx.Request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) &&
                    //        identityManagerOptions.SecurityConfiguration.RequireSsl)
                    //    {
                    //        //ctx.Response.Body.Write("HTTPS required");
                    //       await next();
                    //    }
                    //    else
                    //    {
                    //        await next();
                    //    }
                    //});


                    //options.Value.SecurityConfiguration.Configure(app);

                    //app.MapHttpAttributeRoutes();
                    app.UseFileServer(new FileServerOptions
                    {
                        FileProvider = new EmbeddedFileProvider(typeof(SecurityConfiguration).Assembly,
                            "TzIdentityManager.Assets"),
                        RequestPath = new PathString("/assets"),
                        //FileSystem = new EmbeddedResourceFileSystem(typeof(IdentityManagerAppBuilderExtensions).Assembly, "IdentityManager.Assets")
                    });
                    app.UseFileServer(new FileServerOptions
                    {
                        FileProvider = new EmbeddedFileProvider(typeof(SecurityConfiguration).Assembly,
                            "TzIdentityManager.Assets.Content.fonts"),
                        RequestPath = new PathString("/assets/libs/fonts"),
                        //FileSystem = new EmbeddedResourceFileSystem(typeof(IdentityManagerAppBuilderExtensions).Assembly, "IdentityManager.Assets.Content.fonts")
                    });

                    //app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                    //{
                    //    Authority = "https://localhost:5000/openid"
                    //});
                    app.UseStaticFiles();


                    //SuppressDefaultHostAuthentication();
                    app.Use(async (ctx, next) =>
                    {
                        ctx.User = new ClaimsPrincipal(new ClaimsIdentity());
                        await next.Invoke();
                    });

                    app.UseAuthentication();

                    app.UseMvc(routes =>
                    {
                        routes.MapRoute(Constants.RouteNames.Home, "{controller=Page}/{action=Index}");
                        routes.MapRoute(Constants.RouteNames.Logout, "{controller=Page}/{action=Logout}");
                    });
                    //app.UseStageMarker(PipelineStage.MapHandler);  
                }
            );
            







           

        }










        public static IApplicationBuilder UseBranchWithServices(this IApplicationBuilder app, PathString path,
                Action<IServiceCollection> servicesConfiguration, Action<IApplicationBuilder> appBuilderConfiguration)
            {
                
                var webHost = new WebHostBuilder().UseKestrel().ConfigureServices(servicesConfiguration).UseStartup<EmptyStartup>().Build();
                var serviceProvider = webHost.Services;
                var serverFeatures = webHost.ServerFeatures;

                var appBuilderFactory = serviceProvider.GetRequiredService<IApplicationBuilderFactory>();
                var branchBuilder = appBuilderFactory.CreateBuilder(serverFeatures);
                var factory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

                branchBuilder.Use(async (context, next) =>
                {
                    using (var scope = factory.CreateScope())
                    {
                        context.RequestServices = scope.ServiceProvider;
                        await next();
                    }
                });

                appBuilderConfiguration(branchBuilder);

                var branchDelegate = branchBuilder.Build();

                return app.Map(path, builder =>
                {
                    builder.Use(async (context, next) =>
                    {
                        await branchDelegate(context);
                    });
                });
            }

            private class EmptyStartup
            {
                public void ConfigureServices(IServiceCollection services) { }

                public void Configure(IApplicationBuilder app) { }
            }
        


    }
}
