@echo off
cd /d "%~dp0Game"

set "__COMPAT_LAYER=RUNASINVOKER"

if exist "S4ClientLocal.exe" (
    start "" S4ClientLocal.exe -rc:eu -lac:eng -auth_server_ip:localhost "-key:0|0|0" -aeria_acc_code:123
) else (
    start "" S4Client.exe -rc:eu -lac:eng -auth_server_ip:localhost "-key:0|0|0" -aeria_acc_code:123
)