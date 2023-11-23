taskkill /im widgets.exe /F
taskkill /im WingetUIWidgetProvider.exe /F
cd src
rmdir /Q /S bin
cd Package
rmdir /Q /S bin
pause