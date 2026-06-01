start "Redis" /min "./redis/redis-server.exe"
start "Game Server" /min dotnet exec "./Server/Game/Netsphere.Server.Game.dll"
timeout /t 2 /nobreak >nul
start "Auth Server" /min dotnet exec "./Server/Auth/Netsphere.Server.Auth.dll"
start "Chat Server" /min dotnet exec "./Server/Chat/Netsphere.Server.Chat.dll"
start "Relay Server" /min dotnet exec "./Server/Relay/Netsphere.Server.Relay.dll"