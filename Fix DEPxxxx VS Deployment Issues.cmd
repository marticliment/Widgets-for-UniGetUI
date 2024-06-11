taskkill /im widgets.exe /F
taskkill /im "Widgets for UniGetUI.exe" /F
cd src/Widgets
rmdir /Q /S bin
cd ..
cd Package
rmdir /Q /S bin
pause