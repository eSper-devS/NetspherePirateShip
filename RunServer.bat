start "Redis" "./redis/redis-server.exe"
start "Game Server" dotnet exec "./Server/Game/Netsphere.Server.Game.dll"
timeout /t 2 /nobreak >nul
start "Auth Server" dotnet exec "./Server/Auth/Netsphere.Server.Auth.dll"
start "Chat Server" dotnet exec "./Server/Chat/Netsphere.Server.Chat.dll"
start "Relay Server" dotnet exec "./Server/Relay/Netsphere.Server.Relay.dll"