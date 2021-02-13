using HonzaBotner.Discord.Services.Commands;
using HonzaBotner.Database;
using HonzaBotner.Discord;
using HonzaBotner.Discord.Services;
using HonzaBotner.Discord.Services.Reactions;
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

            services.AddDbContext<HonzaBotnerDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HonzaBotner")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "HonzaBotner", Version = "v1"});
            });

            services.AddBotnerServicesOptions(Configuration)
                .AddHttpClient()
                .AddBotnerServices();

            services.AddDiscordOptions(Configuration)
                .AddCommandOptions(Configuration)
                .AddDiscordBot(config =>
                {
                    config.RegisterCommands<AuthorizeCommands>();
                    config.RegisterCommands<AdminCommands>();
                    config.RegisterCommands<OtherCommands>();
                    config.RegisterCommands<EmoteCommands>();
                    // config.AddCommand<HiCommand>(HiCommand.ChatCommand);
                    // config.AddCommand<AuthorizeCommand>(AuthorizeCommand.ChatCommand);
                    // config.AddCommand<CountCommand>(CountCommand.ChatCommand);
                    // config.AddCommand<Activity>(Activity.ChatCommand);
                    // config.AddCommand<HugCommand>(HugCommand.ChatCommand);
                    // // Messages
                    // config.AddCommand<SendMessage>(SendMessage.ChatCommand);
                    // config.AddCommand<EditMessage>(EditMessage.ChatCommand);
                    // config.AddCommand<SendImage>(SendImage.ChatCommand);
                    // config.AddCommand<EditImage>(EditImage.ChatCommand);
                    // // Polls
                    // config.AddCommand<PollCommand>(PollCommand.ChatCommand);
                }, reactions =>
                {
                    reactions.AddReaction<VerificationReactionHandler>()
                        .AddReaction<StaffVerificationReactionHandler>()
                        .AddReaction<EmojiCounterHandler>();
                }, collection =>
                {
                    collection.AddSingleton<IVoiceManager, VoiceManager>();
                });
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
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<HonzaBotnerDbContext>();
            context?.Database.Migrate();
        }
    }
}
