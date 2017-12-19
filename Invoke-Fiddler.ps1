<#
    Assumes that Fiddler Core Libraries are in same directory as this script.
    http://www.telerik.com/fiddler/fiddlercore
    This script uses Fiddler Core 4
#>
 
function Start-Fiddler {
<#
.Synopsis
 Uses FiddlerCore to listen on a specified port.
 
.Description
 Start-Fiddler loads the FiddlerCore DLL and uses Fiddler.FiddlerApplication to listen on a specified port.
 When http(s) traffic is generated Fiddler logs the traffic. The result is exposed through a job interface.
 Start-Fiddler requires FiddlerCore which allows you to integrate HTTP/HTTPS traffic viewing and modification capabilities into your .NET application.
 
.PARAMETER ListenPort
 Specifies the Port that Fiddler listens to.
 
.PARAMETER RegisterAsSystemProxy
 Registers as the system proxy, default set to False.
 
.Example
 Start-Fiddler -ListenPort 8877 -RegisterAsSystemProxy
 Starts Fiddler and listens to Port 8877, registers as the system proxy.
 
.Example
 Start-Fiddler -ListenPort 8877 -RegisterAsSystemProxy -Whatif
 Displays what would happen if you run Start-Fiddler.
 
.NOTES
  Start-Fiddler requires FiddlerCore which allows you to integrate HTTP/HTTPS traffic viewing and modification capabilities into your .NET application.
 
.LINK
  https://www.fiddler2.com/fiddler/core/
#>
 
  [cmdletbinding(SupportsShouldProcess = $true)]
  param(
    [Parameter(
      Mandatory = $true,
      Position = 0)]
    [int]$ListenPort,
    [switch]$RegisterAsSystemProxy
  )
 
  Process {
    Try {
      # Start FiddlerApplication
      if(-not([Fiddler.FiddlerApplication]::IsStarted())) {
        if($psCmdlet.ShouldProcess("[Fiddler.FiddlerApplication]","Startup")) {
          $FiddlerCoreStarupFlags = [Fiddler.FiddlerCoreStartupFlags]::DecryptSsl -band [Fiddler.FiddlerCoreStartupFlags]::RegisterAsSystemProxy -band [Fiddler.FiddlerCoreStartupFlags]::ChainToUpstreamProxy 
          #[Fiddler.FiddlerApplication]::StartUp($ListenPort,$RegisterAsSystemProxy,$true) #This is the deprecated calling Convention.  New version uses FiddlerCoreStartupFlags
          [Fiddler.FiddlerApplication]::StartUp($ListenPort,$RegisterAsSystemProxy,$true) 
           
        }
      } else {
        Write-Verbose "FiddlerApplication is already started"
      }
    }
    Catch {
      $error[0]
      Continue
    }
    Try {
      if(-not(Get-EventSubscriber | Where-Object { $_.EventName -eq "BeforeRequest" })) {
        if($psCmdlet.ShouldProcess("BeforeRequest","Register-ObjectEvent")) {
          $fiddlerApplication = [Fiddler.FiddlerApplication]
          # Register Event
          $fiddlerApplicationBeforeRequest = Register-ObjectEvent -InputObject $fiddlerApplication -EventName 'BeforeRequest' -Action { 
            $args | Select-Object *;
          }
          # Store SourceIdentifier in Script Variable
          $script:FiddlerEventIdentifier = (Get-EventSubscriber | Where-Object { $_.EventName -eq "BeforeRequest" }).SourceIdentifier
          # Store job in Script Variable
          $script:FiddlerJobID = $fiddlerApplicationBeforeRequest.Id
        }
      } else {
        Write-Verbose "Eventsubscriber already exists"
      }
    }
    Catch {
      $error[0]
      Continue
    }
  }
}
 
function Stop-Fiddler {
 
<#
.Synopsis
 Stops Fiddler.
 
.Description
 Stop-Fiddler Unregisters the Fiddler Event, Removes the Jobs associated with it and Clears the Script Variables used between the functions.
 
.Example
 Stop-Fiddler
 Unregisters the Fiddler event, Removes any Jobs associated with the event and clears the Script Variables used.
 
.Example
 Stop-Fiddler -Verbose
 Unregisters the Fiddler event, Removes any Jobs associated with the event and clears the Script Variables used and writes a verbose messages.
 
.Example
 Stop-Fiddler -Whatif
 Displays what would happen if you run Stop-Fiddler.
 
.NOTES
  Stop-Fiddler requires FiddlerCore which allows you to integrate HTTP/HTTPS traffic viewing and modification capabilities into your .NET application.
 
.LINK
  https://www.fiddler2.com/fiddler/core/
#>
 
  [cmdletbinding(SupportsShouldProcess = $true)]
  param()
 
  # Unregister Event
  if(Get-EventSubscriber | Where-Object { $_.SourceIdentifier -eq $FiddlerEventIdentifier }) {
    if($psCmdlet.ShouldProcess($FiddlerEventIdentifier,"Unregister-Event")) {
      Get-EventSubscriber -SourceIdentifier $FiddlerEventIdentifier | Unregister-Event
      Write-Verbose "FiddlerEvent $FiddlerEventIdentifier unregistered"
    }
  }
 
  # Stop and Remove Jobs
  if(Get-Job | Where-Object { $_.Id -eq $fiddlerJobId }) {
    if($psCmdlet.ShouldProcess($fiddlerJobId,"Stop-Job")) {
      Get-Job -Id $fiddlerJobId | Stop-Job
      Write-Verbose "FiddlerJob: $fiddlerJobId Stopped"
    }
    if($psCmdlet.ShouldProcess($fiddlerJobId,"Remove-Job")) {
      Get-Job -Id $fiddlerJobId | Remove-Job -Force
      Write-Verbose "FiddlerJob: $fiddlerJobId Removed"
    }
  }
 
  # Shutdown Fiddler
  if([appdomain]::currentdomain.GetAssemblies() | Where { $_.ManifestModule.ToString() -eq "FiddlerCore.dll" }) {
    if($psCmdlet.ShouldProcess("[Fiddler.FiddlerApplication]","ShutDown")) {
      [Fiddler.FiddlerApplication]::Shutdown()
      Write-Verbose "FiddlerApplication shutdown"
    }
  } else {
    Write-Warning "FiddlerCore not added. Unable to run Shutdown() method."
  }
  # Nullify Script Variables
  if($psCmdlet.ShouldProcess("FiddlerVariables","Clear-Variable")) {
    $script:FiddlerEventIdentifier = $null
    $script:FiddlerJobID = $null
  }
}
 
function Receive-Fiddler {
 
<#
.Synopsis
 Gets the results of the Fiddler background job in the current session.
 
.Description
 Receive-Fiddler gets the results of the Windows PowerShell background jobs in the current session.
 By default, the result is deleted from the system when you receive them, you can use the Keep parameter
 to save the results so that you can receive them again.
 
.Example
 Receive-Fiddler
 Gets the results from a Fiddler job.
 
.Example
 Receive-Fiddler -Keep
 Gets the results from a Fiddler job and saves the results so that you can receive them again.
 
.Example
 Receive-Fiddler -Whatif
 Displays what would happen if you run Receive-Fiddler.
 
.NOTES
  Receive-Fiddler requires FiddlerCore which allows you to integrate HTTP/HTTPS traffic viewing and modification capabilities into your .NET application.
 
.LINK
  https://www.fiddler2.com/fiddler/core/
#>
 
  [cmdletbinding(SupportsShouldProcess = $true)]
  param([switch]$Keep)
  if($fiddlerJobId -is [int]) {
    if(Get-Job | Where-Object { $_.Id -eq $fiddlerJobId }) {
      if($psCmdlet.ShouldProcess($fiddlerJobId,"Receive-Job")) {
        Receive-Job -Id $fiddlerJobId -Keep:$Keep
      }
    }
  }
}
 
# Write a Loop, or Just Embed Base64 For an all in one script
# I left it explicit here so it was clear what is being loaded.
 
$Content = Get-Content -Path FiddlerCore4.dll -Encoding Byte
$FiddlerCore4Dll = [System.Convert]::ToBase64String($Content)
[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String($FiddlerCore4Dll))
$Content = Get-Content -Path Certmaker.dll -Encoding Byte
$CertMakerDll = [System.Convert]::ToBase64String($Content)
[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String($CertMakerDll))
$Content = Get-Content -Path BCMakeCert.dll -Encoding Byte
$BCMakeCertDll = [System.Convert]::ToBase64String($Content)
[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String($BCMakeCertDll))
 
Write-Verbose 'Fiddler Core Assemblies Loaded'
Start-Fiddler -ListenPort 8888 -RegisterAsSystemProxy -Verbose
while($true)
{
    Receive-Fiddler -Keep
}