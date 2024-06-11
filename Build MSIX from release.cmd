@echo off

echo BEFORE RUNNING THIS COMMAND THE SOLUTION MUST BE BUILT WITH THE CONFIGURATION RELEASE x64!

pause

cd src
cd Package
cd bin
cd x64
del "Widgets for UniGetUI Installer.msix"
cd Release

cd "Widgets for UniGetUI"

del Microsoft.WinUI.dll
del Microsoft.InteractiveExperiences.Projection.dll

echo "You may want to change now MSVC++ Redist UWP version"
pause 

cd ..
cd ..

MakeAppx.exe pack /d Release /p "installer.msix"
move "installer.msix" ..\..\..\..\

cd ..\..\..\..\

rem Installer will be signed by Microsoft Store
rem "Y:\- Signing\signtool-x64\signtool.exe" sign /v /debug /fd SHA256 /tr "http://timestamp.acs.microsoft.com" /td SHA256 /dlib "Y:\- Signing\azure.codesigning.client\x64\Azure.CodeSigning.Dlib.dll" /dmdf "Y:\- Signing\metadata.json" "Widgets for UniGetUI Installer.msix"

del "Widgets for UniGetUI Installer.msix"
move "installer.msix" "Widgets for UniGetUI Installer.msix"

pause