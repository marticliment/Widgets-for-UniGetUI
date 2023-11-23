@echo off

echo BEFORE RUNNING THIS COMMAND THE SOLUTION MUST BE BUILT WITH THE CONFIGURATION RELEASE x64!

pause

del "WingetUI Widgets.msix"
cd src
cd Package
cd bin
cd x64

cd Release

move .\Package\Images .\Images
rmdir /Q /S Package
cd WingetUIWidgetProvider

rmdir /Q /S .\Package
del Microsoft.WinUI.dll
del Microsoft.InteractiveExperiences.Projection.dll

cd ..
cd ..

MakeAppx.exe pack /d Release /p "WingetUI Widgets.msix"
move "WingetUI Widgets.msix" ..\..\..\..\
echo You may want to sign your installer now
pause