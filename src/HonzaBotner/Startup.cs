using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using HonzaBotner.Discord.Services.Commands;
using HonzaBotner.Discord.Services.Commands.Messages;
using HonzaBotner.Discord.Services.Commands.Pools;
using HonzaBotner.Database;
using HonzaBotner.Discord;
using HonzaBotner.Services;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

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
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddControllers();
            services.AddDbContext<HonzaBotnerDbContext>(options =>
                options.UseNpgsql(
                    PsqlConnectionStringParser.GetEFConnectionString(
                        Configuration["DATABASE_URL"]
                    ), b => b.MigrationsAssembly("HonzaBotner")));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "HonzaBotner", Version = "v1"});
            });

            services.AddDiscordOptions(Configuration)
                .AddDiscordBot(config =>
                {
                    config.AddCommand<HiCommand>(HiCommand.ChatCommand);
                    config.AddCommand<AuthorizeCommand>(AuthorizeCommand.ChatCommand);
                    config.AddCommand<CountCommand>(CountCommand.ChatCommand);
                    config.AddCommand<Activity>(Activity.ChatCommand);
                    // Messages
                    config.AddCommand<SendMessage>(SendMessage.ChatCommand);
                    config.AddCommand<EditMessage>(EditMessage.ChatCommand);
                    config.AddCommand<SendImage>(SendImage.ChatCommand);
                    config.AddCommand<EditImage>(EditImage.ChatCommand);
                    // Pools
                    config.AddCommand<YesNo>(YesNo.ChatCommand);
                    config.AddCommand<Abc>(Abc.ChatCommand);
                });

            services.AddBotnerServicesOptions(Configuration)
                .AddHttpClient()
                .AddBotnerServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(delegate(SwaggerUIOptions c)
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HonzaBotner v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
