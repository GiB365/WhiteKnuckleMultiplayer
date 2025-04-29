
# Multiplayer White Knuckle Mod

A "working" multiplayer mod for White Knuckle.


## Features

- Joining and hosting lobbies
- Ugly player model
- Position + rotation synchronization
- Same seed for the first run


## FAQ

#### How to host a server?

    1. Open the command console by pressing "Shift+~"  
    2. Use the "cheats" command to enable cheats.
    3. Use the "host" command to start a server.
        Usage: host [port]
        Ex: host 7777

#### How to join a server?

    1. Open the command console by pressing "Shift+~"  
    2. Use the "cheats" command to enable cheats.
    3. Use the "join" command to start a client.
        Usage: join [ip] [port]
        Ex: join 127.0.0.1 7777

#### Stuck on "Trying to join ip: [ip]..." / Not joining server.

    1. Check that you used the server's public ip.
    2. Join after the server has been hosted or it breaks the mod (Sorry... I'm working on it.)
    3. Check that the host has port-forwarded
    4. Put the Player.Log file at C:\Users\<YourUsername>\AppData\LocalLow\Dark Machine\White Knuckle\Player.log in the issues.

## Todo

- [x]  Hosting
- [x]  Joining
- [ ]  Leaving
- [x]  Position sync
- [x]  Rotation sync
- [x]  Seed sync when connecting
- [ ]  Stay on seed after death
- [ ]  Better player model
- [ ]  Player death particles

### Future ideas
- Text chat with command console
- Synchronized mass
- Spectate on death instead of respawning
- Different gamemodes (spectate/no spectate)
- Proximity voice chat (probably not happening)
