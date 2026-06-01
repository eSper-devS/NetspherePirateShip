# NetspherePirateShip
S4 League server emulator for EU v1267 - Season 8
Fork of https://gitlab.com/NetspherePirates/NetspherePirates/ but modified enough to make selfhosting way simpler

##THIS IS ONLY FOR DEVELOPMENT, DO NOT USE FOR HOSTING PUBLIC SERVERS

### Requirements
* [.Net Core 2.1](https://www.microsoft.com/net/download/dotnet-core/2.1)
* **Windows only:** [.Net Framework 4.7.2](https://www.microsoft.com/net/download/thank-you/net472)
* [Redis](https://redis.io/) 
* S4 League Client - Season 8 with exposed resources(EU v1267): [Download(https://archive.org/download/s4lgameclientarchives/S4%20League%20Game%20Client%20Archive/Season%208(EU%20v1267)/ExternalResourceFumbiClient_1267_S8.7z)

If you want a seamless install you need to put Redis and the client in their respective folders, otherwise the .bat files wont work and the server wont find the client resources.

### How to run
You only need to put redis and the client in the specified folders, once that's done you only need to compile and run the .bat files


### Whats different
These modifications are for the following:

No need to install anything aside .Net and download redis
DB created on startup if it doesnt exist
DB checks if channels, shop prices and shop items tables are empty, if anything is missing it adds everything automatically from the game files
This means the in-game shop is ready to go from the start, and it can be refreshed by deleting game.db
Server makes account if it doesnt exist, if it does it just overrides the password
On first log in character is created automatically and equiped with default stuff defined in start_items (dagger and wings by default)
Max lvl on account creation with all chars created
Can enter any type of gamemode alone
No AFK kick


The referenced client has the following modifications:
Resources exposed, and the server uses those exact same resources if the client is placed in the "Game" folder
Can modify most resources on the fly so no need to restart the client that frequently, only for weapons .lua files 
No mission pop up on log in

### Extra info
I took the liberty to disable a lot of common logs, can check which ones looking for:
"//Disabled log to avoid filling console with too much stuff"
The files in dbBackup are just empty databases and SQL commands to make them in MySQL and SQLite, in case anyone wants that

### Credits
Thanks EngineLessCC for her help making the client have the resources exposed
Thanks to wtfblub and EngineLessCC for their help on this server code