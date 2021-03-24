using HonzaBotner.Discord.Services.Commands;
using HonzaBotner.Database;
using HonzaBotner.Discord;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services;
using HonzaBotner.Discord.Services.EventHandlers;
using HonzaBotner.Discord.Services.Managers;
using HonzaBotner.Services;
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

            string connectionString = PsqlConnectionStringParser.GetEFConnectionString(Configuration["DATABASE_URL"]);

            services
                .AddDbContext<HonzaBotnerDbContext>(options =>
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HonzaBotner"))
                )

                // Swagger
                .AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "HonzaBotner", Version = "v1"}); })

                // Botner
                .AddBotnerServicesOptions(Configuration)
                .AddHttpClient()
                .AddBotnerServices()

                // Discord
                .AddDiscordOptions(Configuration)
                .AddCommandOptions(Configuration)
                .AddDiscordBot(config =>
                    {
                        //config.RegisterCommands<AuthorizeCommands>();
                        config.RegisterCommands<BotCommands>();
                        config.RegisterCommands<ChannelCommands>();
                        config.RegisterCommands<EmoteCommands>();
                        config.RegisterCommands<FunCommands>();
                        config.RegisterCommands<MemberCommands>();
                        config.RegisterCommands<MessageCommands>();
                        config.RegisterCommands<PollCommands>();
                        config.RegisterCommands<TestCommands>();
                        config.RegisterCommands<VoiceCommands>();
                        config.RegisterCommands<WarningCommands>();
                    }, reactions =>
                    {
                        reactions
                            .AddEventHandler<BoosterHandler>()
                            .AddEventHandler<EmojiCounterHandler>()
                            .AddEventHandler<HornyJailHandler>()
                            .AddEventHandler<NewChannelHandler>()
                            .AddEventHandler<PinHandler>()
                            .AddEventHandler<RoleBindingsHandler>(EventHandlerPriority.High)
                            .AddEventHandler<StaffVerificationEventHandler>(EventHandlerPriority.Urgent)
                            .AddEventHandler<VerificationEventHandler>(EventHandlerPriority.Urgent)
                            .AddEventHandler<VoiceHandler>()
                            ;
                    }
                )

                // Managers
                .AddTransient<IVoiceManager, VoiceManager>()
                ;
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
            else
            {
                UpdateDatabase(app);
                app.UseReverseProxyHttpsEnforcer();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using HonzaBotnerDbContext? context = serviceScope.ServiceProvider.GetService<HonzaBotnerDbContext>();
            context?.Database.Migrate();
        }
    }
}
