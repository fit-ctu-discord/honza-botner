using System;
using System.Net.Http;
using System.Security.Claims;
using HonzaBotner.Commands;
using HonzaBotner.Data;
using HonzaBotner.Discord;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HonzaBotner
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
                options.UseNpgsql(Configuration["CVUT:ConnectionString"]));
            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddAuthentication("CVUT")
               .AddOAuth("CVUT", "CVUT Login", options =>
               {
                   options.AuthorizationEndpoint = "https://auth.fit.cvut.cz/oauth/authorize";
                   options.TokenEndpoint = "https://auth.fit.cvut.cz/oauth/token";
                   options.UserInformationEndpoint = "https://auth.fit.cvut.cz/oauth/check_token";

                   options.CallbackPath = "/signin-oidc";

                   options.Scope.Add("urn:ctu:oauth:umapi.read");
                   options.Scope.Add("cvut:umapi:read");

                   options.ClientId = Configuration["CVUT:ClientId"];
                   options.ClientSecret = Configuration["CVUT:ClientSecret"];

                   var innerHandler = new HttpClientHandler();
                   options.BackchannelHttpHandler = new AuthorizingHandler(innerHandler, options);
                   options.Events = new OAuthEvents
                   {
                       OnCreatingTicket = async context =>
                       {
                           var uriBuilder = new UriBuilder(context.Options.UserInformationEndpoint);
                           uriBuilder.Query = $"token={context.AccessToken}";
                           var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

                           HttpResponseMessage response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                           response.EnsureSuccessStatusCode();

                           var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                           string? userName = user.RootElement.GetProperty("user_name").GetString();
                           if (userName == null)
                               throw new InvalidOperationException();

                           context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, $"{userName}@fit.cvut.cz"));
                           context.Identity.AddClaim(new Claim(ClaimTypes.Email, $"{userName}@fit.cvut.cz")); // HACK: FIX IT
                           context.Identity.AddClaim(new Claim(ClaimTypes.Name, userName));

                           context.RunClaimActions();
                       }
                   };
               });
            services.AddRazorPages(config =>
            {
                //config.Conventions.AuthorizePage("Auth");
            });

            services.AddDiscordOptions(Configuration);
            services.AddDiscordBot(config =>
            {
                // TODO: Commands here
                config.AddCommand<HiCommand>(HiCommand.ChatCommand);
                config.AddCommand<SendMessageCommand>(SendMessageCommand.ChatCommand);
                config.AddCommand<SendImageCommand>(SendImageCommand.ChatCommand);
                config.AddCommand<AuthorizeCommand>(AuthorizeCommand.CommandText);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
