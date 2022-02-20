# Voice

#### `::voice add`

Adds a custom voice channel,
that will be available to join for the next 30 seconds.
After somebody joins, it stays in the server until somebody is in it.

#### `::voice edit`

In a custom voice channel
the name and/or limitations of such a channel
can also be edited.
Be aware that Discord has rate limits
that prevent editing multiple times in a row.

#### Custom voice deletion

Whenever a user leaves a custom voice channel,
we check if somebody is still there and if not,
the bot removes the channel.
This feature helps us keep custom voice channels clean of unused channels.
