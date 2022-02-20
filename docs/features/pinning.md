# Pinning

Similarly to [role granting](granting-roles.md),
we handle several reactions:

- 📌 (`:pushpin:`), aka "soft pin"
- 📍 (`:round_pushpin:`), aka "hard pin"
- 🔒 (`:lock`), aka "anti pin"

If reacting using 📌 or 📍 reaches the specific threshold,
the message is automatically pinned.
- Using 📌 means that this pinned message will be unpinned on moderator's command.
- Using 📍 keeps the message pinned forever.

If 🔒 is used by the bot or a moderator,
the message is unpinned and is locked from further pinning.
