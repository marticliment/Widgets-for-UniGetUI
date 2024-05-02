@echo off

echo BEFORE RUNNING THIS COMMAND THE SOLUTION MUST BE BUILT WITH THE CONFIGURATION RELEASE x64!

pause

cd src
cd Package
cd bin
cd x64
del "WingetUI Widgets.msix"
cd Release

cd "Widgets for UniGetUI"

del Microsoft.WinUI.dll
del Microsoft.InteractiveExperiences.Projection.dll

cd ..
cd ..

MakeAppx.exe pack /d Release /p "WingetUI Widgets.msix"
move "WingetUI Widgets.msix" ..\..\..\..\

cd ..\..\..\..\

rem Installer will be signed by Microsoft Store
rem "Y:\- Signing\signtool-x64\signtool.exe" sign /v /debug /fd SHA256 /tr "http://timestamp.acs.microsoft.com" /td SHA256 /dlib "Y:\- Signing\azure.codesigning.client\x64\Azure.CodeSigning.Dlib.dll" /dmdf "Y:\- Signing\metadata.json" "WingetUI Widgets.msix"

del "WingetUI Widgets Installer.msix"
move "WingetUI Widgets.msix" "WingetUI Widgets Installer.msix"

pause