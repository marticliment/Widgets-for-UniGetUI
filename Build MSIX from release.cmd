@echo off

echo BEFORE RUNNING THIS COMMAND THE SOLUTION MUST BE BUILT WITH THE CONFIGURATION RELEASE x64!

pause

cd src
cd Package
cd bin
cd x64
del "WingetUI Widgets.msix"
cd Release

rmdir /Q /S .\Images
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

"Y:\- Signing\signtool-x64\signtool.exe" sign /v /debug /fd SHA256 /tr "http://timestamp.acs.microsoft.com" /td SHA256 /dlib "Y:\- Signing\azure.codesigning.client\x64\Azure.CodeSigning.Dlib.dll" /dmdf "Y:\- Signing\metadata.json" "WingetUI Widgets.msix"

pause