# W40kDoWSS-GameSpy
GameSpy emulation, Steam connection retranslator, and other services to play Warhammer 40k DoW through the Internet on your own server with nice Launcher!

## Testing at LOCALHOST

1) Start GSMasterServer project (its server).

2) Update App.cs in SteamSpy project. It contains hosts modification code. Change in to localhost implementation.

3) Update address in GameConstants.cs to localhost.

3) Run Steam and log in.

4) Start SteamSpy project (its retranslator)

5) Start Soulstorm 1.2 version and connect via internet menu option.

6) Play!

## Dowstats integration
To setup custom dowstats server add to server environment variable "dowstatsServer" (someAnotherDowstats.ru) and "dowstatsVersion" (108).  Without environment paramethers game stats will send to dowstats.ru. 
