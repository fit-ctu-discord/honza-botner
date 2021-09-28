[tenhobi]: https://github.com/tenhobi
[ostorc]: https://github.com/ostorc
[stepech]: https://github.com/stepech

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2021-09-28

### Fixed

- Hangfire no longer breaks the app due to Heroku db connection limit. Number of workers have been limited to 3, db connections stays at 14/20. (by [@tenhobi][tenhobi])
- Path to FIT Discord logo in auth page. (by [@tenhobi][tenhobi])

## [1.0.0] - 2021-09-28

### Added

- Logging to Discord channel. ([#158](https://github.com/fit-ctu-discord/honza-botner/issues/158), by [@ostorc][ostorc])
- Verification using buttons. (by [@stepech][stepech])
- Reminder command. ([#7](https://github.com/fit-ctu-discord/honza-botner/issues/7), by [@jirkavrba](https://github.com/jirkavrba) and [@tenhobi][tenhobi])
- Voice command and event handler, containing auto creating voice channel by joining special voice channel or using command to create and edit it. (by [@tenhobi][tenhobi])
- Granting roles event handler. (by [@ostorc][ostorc])
- Welcome message handler.  (by [@tenhobi][tenhobi])
- Warning command, containing adding and listing warning to guild members. (by [@tenhobi][tenhobi])
- Bot command, containing activity changing and bot info. (by [@tenhobi][tenhobi] and [@stepech][stepech])
- Channel command, containing channel cloning. (by [@tenhobi][tenhobi])
- Member command, containing guild member counting, getting info, and removing. (by [@tenhobi][tenhobi])
- Message command, containing message sending, editing, removing and binding handler to some reaction. (by [@tenhobi][tenhobi] and [@ostorc][ostorc])
- Fun command, containing random picker of provided options. (by [@tenhobi][tenhobi])
- User pinning, containing event handler, soft pin pruning etc. ([#181](https://github.com/fit-ctu-discord/honza-botner/pull/181), by [@tenhobi][tenhobi] and [@stepech][stepech])
- Poll command, containing ABC or yes/no polls. (by [@albru123](https://github.com/albru123), [@ostorc][ostorc] and [@tenhobi][tenhobi])
- Emotes counting command and event handler. (by [@ostorc][ostorc])
- Authorization using CTU OAuth2. (by [@ostorc][ostorc])
- Core functionality and design of the bot. (by [@ostorc][ostorc])
