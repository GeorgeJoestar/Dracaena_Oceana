@echo off
setlocal EnableDelayedExpansion

dir .. /b /ad | findstr /I "Fishery" >nul
if %errorlevel%==0 (
    echo A directory containing "Fishery" already exists. Skipping Fishery installation.
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path 'Fishery-main.zip' -DestinationPath 'temp_extract'"
    move temp_extract\Fishery-main ..\
    rmdir /s /q temp_extract
)

set "foundPF="
for /f "delims=" %%d in ('dir .. /b /ad') do (
    echo %%d | findstr /I "performance" >nul
    if !errorlevel! equ 0 (
        echo %%d | findstr /I "fish" >nul
        if !errorlevel! equ 0 (
            set "foundPF=%%d"
        )
    )
)
if defined foundPF (
    echo A directory containing both "performance" and "fish" already exists. Skipping Performance-Fish installation.
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path 'Performance-Fish-main.zip' -DestinationPath 'temp_extract'"
    move temp_extract\Performance-Fish-main ..\
    rmdir /s /q temp_extract
)

set "foundAR="
for /f "delims=" %%d in ('dir .. /b /ad') do (
    echo %%d | findstr /I "Alien" >nul
    if !errorlevel! equ 0 (
        echo %%d | findstr /I "Race" >nul
        if !errorlevel! equ 0 (
            set "foundAR=%%d"
        )
    )
)
if defined foundAR (
    echo A directory containing both "Alien" and "Race" already exists. Skipping AlienRaces installation.
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path 'AlienRaces-master.zip' -DestinationPath 'temp_extract'"
    move temp_extract\AlienRaces-master ..\
    rmdir /s /q temp_extract
)

echo Done! The content has been moved to the parent folder.
endlocal
