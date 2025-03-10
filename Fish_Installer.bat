@echo off
powershell -NoProfile -ExecutionPolicy Bypass ^
  -Command "Expand-Archive -Path 'Fishery-main.zip' -DestinationPath 'temp_extract'"
move temp_extract\Fishery-main ..\
rmdir /s /q temp_extract
powershell -NoProfile -ExecutionPolicy Bypass ^
  -Command "Expand-Archive -Path 'Performance-Fish-main.zip' -DestinationPath 'temp_extract'"
move temp_extract\Performance-Fish-main ..\
rmdir /s /q temp_extract
echo Done! The content has been moved to the parent folder.
