# HonzaBotner

Bot for unoffici

## Configuration

[`secrets.json`][secrets]:

```json
{
  "CVUT:ClientId": "test",
  "CVUT:ClientSecret": "test",
  "DATABASE_URL": "Host=localhost;Database=HonzaBotner;Username=honza-bot;Password=postgres",
  "Discord:Token": "test"
}
```


```
dotnet tool install --global dotnet-ef
cd src/HonzaBotner
dotnet ef database update
```


```
dotnet ef migrations add "test"
```


https://auth.fit.cvut.cz/manager/user/apps.xhtml

[secrets]: https://docs.microsoft.com/cs-cz/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows
