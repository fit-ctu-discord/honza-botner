using HonzaBotner.Database;
using HonzaBotner.Discord;
using HonzaBotner.Discord.EventHandler;
using HonzaBotner.Discord.Managers;
using HonzaBotner.Discord.Services;
using HonzaBotner.Discord.Services.Commands;
using HonzaBotner.Discord.Services.EventHandlers;
using HonzaBotner.Discord.Services.Jobs;
using HonzaBotner.Discord.Services.Managers;
using HonzaBotner.Discord.Services.Utils;
using HonzaBotner.Discord.Utils;
using HonzaBotner.Scheduler;
using HonzaBotner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace HonzaBotner;

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
        ulong? guildId = Configuration.GetSection("Discord").GetValue<ulong>("GuildId");

        services
            .AddDbContext<HonzaBotnerDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HonzaBotner"))
            )

            // Swagger
            .AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "HonzaBotner", Version = "v1" }); })

            // Botner
            .AddBotnerServicesOptions(Configuration)
            .AddHttpClient()
            .AddBotnerServices()

            // Discord
            .AddDiscordOptions(Configuration)
            .AddCommandOptions(Configuration)
            .AddDiscordBot( reactions =>
                {
                    reactions
                        .AddEventHandler<BoosterHandler>()
                        .AddEventHandler<EmojiCounterHandler>()
                        .AddEventHandler<HornyJailHandler>()
                        .AddEventHandler<NewChannelHandler>()
                        .AddEventHandler<PinHandler>()
                        .AddEventHandler<ReminderReactionsHandler>()
                        .AddEventHandler<RoleBindingsHandler>(EventHandlerPriority.High)
                        .AddEventHandler<StaffVerificationEventHandler>(EventHandlerPriority.Urgent)
                        .AddEventHandler<VerificationEventHandler>(EventHandlerPriority.Urgent)
                        .AddEventHandler<VoiceHandler>()
                        .AddEventHandler<BadgeRoleHandler>()
                        ;
                }, slash =>
                {
                    slash.RegisterCommands<BotCommands>(guildId);
                    slash.RegisterCommands<EmoteCommands>(guildId);
                    slash.RegisterCommands<FunCommands>(guildId);
                    slash.RegisterCommands<MemberCommands>(guildId);
                    slash.RegisterCommands<MessageCommands>(guildId);
                    slash.RegisterCommands<ModerationCommands>(guildId);
                    slash.RegisterCommands<PinCommands>(guildId);
                    slash.RegisterCommands<PollCommands>(guildId);
                    slash.RegisterCommands<ReminderCommands>(guildId);
                    slash.RegisterCommands<VoiceCommands>(guildId);
                }
            )

            // Utils
            .AddTransient<ITranslation, Translation>()

            // Managers
            .AddTransient<IVoiceManager, VoiceManager>()
            .AddTransient<IReminderManager, ReminderManager>()
            .AddTransient<IButtonManager, ButtonManager>()
            ;

        services.AddScheduler(5000)
            .AddScopedCronJob<TriggerRemindersJobProvider>()
            .AddScopedCronJob<StandUpJobProvider>();
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
            app.UseHttpsRedirection();
        }
        else
        {
            UpdateDatabase(app);
            app.UseReverseProxyHttpsEnforcer();
            app.UseExceptionHandler("/error");
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
