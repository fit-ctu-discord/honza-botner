# Member

#### `::member info`
![](https://img.shields.io/static/v1?label=role&message=mod&color=red)

Provides info (Discord mention, CTU nickname hash)
so we can better debug and help members with removing old accounts etc.
Using subcommands, it also distinguishes between CTU nickname and Discord mention input.

#### `::member delete`
![](https://img.shields.io/static/v1?label=role&message=mod&color=red)

Deletes the member record by providing CTU nickname or Discord mention.

#### `::member count`
![](https://img.shields.io/static/v1?label=role&message=mod&color=red)
![](https://img.shields.io/static/v1?label=role&message=teacher&color=red)

Counts number of people having specific role(s).
Using sub commands also use `and` and `or` operators.
All authenticated members can be counted using `all` subcommand.

### Automatic assignment of badge roles

This feature is useful when we want to visually distinguish different member roles,
but using only colors is no longer sufficient enough.
Bot checks if a given user has any of the `TriggerRoles`
(generally roles that use color to differentiate)
and if so, it checks for a presence of other configured `PairedRoles`.
If any such role is found, bot then assigns corresponding paired role too.
These roles have generally badges, as a mean of other differentiation.

Bot is capable of automatically keeping them up to date if any of the assigned roles change.
