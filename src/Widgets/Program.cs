﻿// Program.cs

using COM;
using System.Runtime.InteropServices;
using WidgetsForUniGetUI;

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("ole32.dll")]

static extern int CoRegisterClassObject(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnk,
            uint dwClsContext,
            uint flags,
            out uint lpdwRegister);

[DllImport("ole32.dll")] static extern int CoRevokeClassObject(uint dwRegister);

Logger.Log("Registering Widget Provider");
uint cookie;

Guid CLSID_Factory = Guid.Parse("34D3940F-84D6-47C5-B446-32D6865D8852");

CoRegisterClassObject(CLSID_Factory, new WidgetProviderFactory<WidgetProvider>(), 0x4, 0x1, out cookie);

Logger.Log("Registered successfully. Press ENTER to exit.");
Console.ReadLine();

if (GetConsoleWindow() != IntPtr.Zero)
{
    Logger.Log("Registered successfully. Press ENTER to exit.");
    Console.ReadLine();
}
else
{
    // Wait until the manager has disposed of the last widget provider.
    using (ManualResetEvent emptyWidgetListEvent = WidgetProvider.GetEmptyWidgetListEvent())
    {
        emptyWidgetListEvent.WaitOne();
    }

    CoRevokeClassObject(cookie);
}