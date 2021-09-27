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

Count's number of people having specific role(s).
Using sub commands also use `and` and `or` operators.
All authenticated members can be counted using `all` subcommand.
