# How to Start

## Prerequisites
Make sure you have the latest .NET Core and ASP.NET installed. You can find
install instructions [here][dotnet].
You'll also need to have docker installed.

## About our configuration
For configuration, we use a combination of the standard `appsettings.json` file and `secrets.json`.
Non-sensitive information is stored in the `appsettings.json` file
and sensitive information is stored in place outside of the git repository.
(for more details see [documentation][secrets])

### App settings

We use four different `appsetings` files.
- `appsetings.json`: common configuration for all instances.
- `appsetings.BotDev.json`: configuration for test server discord
- `appsetings.CvutFit.json`: configuration for main FIT CTU discord server
- `appsetings.Development.json`: configuration for local development.

### Secrets

Example file structure:

[`secrets.json`][secrets]:
```json
{
    "CVUT:ClientId": "<client id>",
    "CVUT:ClientSecret": "<client secret>",
    "CVUT:ServiceId": "<service id>",
    "CVUT:ServiceSecret": "<service secret>",
    "DATABASE_URL": "Host=localhost;Database=HonzaBotner;Username=honza-bot;Password=postgres",
    "Discord:Token": "<discord bot token>"
}
```

CVUT id and secret can be found on [auth manager][oauth].
In case you don't have a project there,
you will need to create one
and then create an app in it (Web application).
This project also needs to have access to the following services:

- `urn:ctu:oauth:umapi.read`
- `cvut:umapi:read`

Discord token can be found on [Discord developer portal][discordDev].

Database URL in the example is working with provided docker image
which can be found in [compose file][compose].

## Initial configuration
To be able to run the bot locally you need to setup some configs.
- Set the Discord token of your test app and the `DATABASE_URL` parameter
in [`secrets.json`][secrets] the other secrets are not needed for your average
development needs.
- Change `Discord.GuildId` in `appsettings.Development.json` to the id of your
test guild.
- Change `CustomVoiceOptions.ClickChannelId` in `appsettings.Development.json` to
an id of a voice channel in your test guild

## Database and migrations

To modify the database scheme we need to process migrations.
For that, we need to install a tool that does that.
This can be done by the following command:

```sh
dotnet tool install --global dotnet-ef
```

We can see generated files in `src/HonzaBotner/Migrations`.
If everything is OK we can update our DB by the following command:

```sh
dotnet ef database update --project ./src/HonzaBotner
```

With this, we will get migration based on our code

- All `DbSet\<T>` in `HonzaBotnerDbContext` will be used and their mappings will be applied

If we want to add migration, we will navigate to the startup project and will add migration:

```sh
cd src/HonzaBotner
dotnet ef migrations add "Name of migration"
```

# Startup

Before starting the bot itself we will need to start DB.

If you are on Linux start the docker service using `systemctl start docker`

Than start the DB using the following command:

```sh
docker-compose up -d
```

if everything went well you will see `Starting postgres-botner ... done`.

After that, you can start up the project in your favorite IDE
or by typing `dotnet run --project ./src/HonzaBotner/`.


[dotnet]: https://dotnet.microsoft.com/download
[compose]: ../docker-compose.yml
[discordDev]: https://discord.com/developers/applications
[oauth]: https://auth.fit.cvut.cz/manager/user/apps.xhtml
[secrets]: https://docs.microsoft.com/cs-cz/aspnet/core/security/app-secrets
