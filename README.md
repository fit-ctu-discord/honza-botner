# HonzaBotner

Bot for unofficial FIT CTU discord server.

## Configuration

For configuration we use combination of standart appsettings.json file and secrets.json.
Non-senesitive informations are stored in appsettings.json file and sensitive informations are stored in place outside of git reposiotry
(for more details see [documentation][secrets])

### App settings

We use four different appsetings files.
- **appsetings.json**: common configuration for all instances.
- **appsetings.BotDev.json**: configuration for test server discord
- **appsetings.CvutFit.json**: configuration for main FIT CTU discord server
- **appsetings.Development.json**: configuration for local development.

### Secrets

Example file structure:

[`secrets.json`][secrets]:
```json
{
    "CVUT:ClientId": "<client id>",
    "CVUT:ClientSecret": "<client secret>",
    "DATABASE_URL": "Host=localhost;Database=HonzaBotner;Username=honza-bot;Password=postgres",
    "Discord:Token": "<discord bot token>"
}
```

CVUT id and secret can be found on [auth manager][oauth]. In case you don't have project there, you will need to create one
and then create app in it (Web application). This projects also need to have access to following services:

- urn:ctu:oauth:umapi.read
- cvut:umapi:read

Discord token can be found on [Discord developer portal][discordDev].

Databse url in example is working with provided docker image which can be found in [compose file][compose]

## Setup

Before you can start working on bot you need to download latest released version of .NET. Availible version can be found [here][dotnet].
We also need to have access to PostgresDB. For development we use docker image, so you will need to download it too, or you can use other
Postgress instance.

## Startip

Before starting the bot itself we will need to start DB by following command:

```sh
docker-compose up -d
```
if everything went well you will see `Starting postgres-botner ... done`.

After that you can stratup the project in your favorite IDE or by typing `dotnet run --project .\src\HonzaBotner\`.

## Migrations

To modify databse scheme we need to add migration. For that we need to install tool whcih does that. This can be done by following command:
```sh
dotnet tool install --global dotnet-ef
```

After that we will navigate to startup project and we will add migration
```sh
cd src/HonzaBotner
dotnet ef migrations add "Name of migration"
```

With this we will get migration based on our code
- All DbSet\<T> in `HonzaBotnerDbContext` will be used and their mappings will be applied

We can see generated files in `src/HonzaBotner/Migrations`.
If everything is OK we can update our DB by following command:
```sh
dotnet ef database update
```

[dotnet]: https://dotnet.microsoft.com/download
[compose]: docker-compose.yml
[discordDev]: https://discord.com/developers/applications
[oauth]: https://auth.fit.cvut.cz/manager/user/apps.xhtml
[secrets]: https://docs.microsoft.com/cs-cz/aspnet/core/security/app-secrets
