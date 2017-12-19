function invmod([System.Numerics.BigInteger] $a,[System.Numerics.BigInteger] $n){   
     
    $exp = $t = $nt = $r = $nr = New-Object System.Numerics.BigInteger
    $exp = [System.Numerics.BigInteger]1
    $t = [System.Numerics.BigInteger]0
    $nt = [System.Numerics.BigInteger]1
    $r = $n
    $nr = $a
    while ($nr -ne [System.Numerics.BigInteger]0) {
        $q = [System.Numerics.BigInteger]::Divide($r,$nr)       
        $tmp = $nt
        $nt = [System.Numerics.BigInteger]::Subtract($t,[System.Numerics.BigInteger]::Multiply($q,$nt))
        $t = $tmp
        $tmp = $nr
        $nr = [System.Numerics.BigInteger]::Subtract($r, [System.Numerics.BigInteger]::Multiply($q,$nr))
        $r = $tmp
    }
    if ($r -gt 1) {return -1}
    if ($t -lt 0) {$t = [System.Numerics.BigInteger]::Add($t,$n)}
    return $t
}
 
$p = $q = $n = $phi = $e = $d = New-Object System.Numerics.BigInteger
 
$r = [System.Numerics.BigInteger]::Parse("1267822572326555807122159576684530178338449545988069238646937967979")
$phi = [System.Numerics.BigInteger]::Parse("1267822572326555807122159576684527925242400650520489423329838558984")
#Public Key
$e = [System.Numerics.BigInteger]::Parse("65537")
 
#Private Key 
$d = invmod $e $phi
Write-Host "N"
Write-Host $r.ToString('x') -fore Cyan
Write-Host "e"
Write-Host $e.ToString('x') -fore Green
Write-Host "d"
Write-Host $d.ToString('x') -fore Yellow 
 
$test = [System.Numerics.BigInteger]::ModPow([System.Numerics.BigInteger]::Multiply($e, $d), [System.Numerics.BigInteger]::Parse("1"), $phi)
Write-Host $test
 
<# RSA Challenge:  http://singularityctf.blogspot.ru/2014/03/backdoorctf-2014-writeup-crypto-100-eng.html  
Cipher Text in Hex:  0c08d1e922a612492045732b00a54640cb252e2e84f0758af387d60c
Public Key
-----BEGIN PUBLIC KEY-----
MDcwDQYJKoZIhvcNAQEBBQADJgAwIwIcDAnn7Hjy+K2plTRIImR3KBsJnRg1cCtN
5QddawIDAQAB
-----END PUBLIC KEY-----
#TODO, write example of extracting Key.
 
Factored here: https://www.alpertron.com.ar/ECM.HTM
 
1267822572326555807122159576684530178338449545988069238646937967979 (67 digits) = 
1090660992520643446103273789680343 (34 digits) × 1162435056374824133712043309728653 (34 digits)
Euler's totient: 1267822572326555807122159576684527925242400650520489423329838558984 (67 digits)
 
#>
 
 
$c = [System.Numerics.BigInteger]::Parse('0c08d1e922a612492045732b00a54640cb252e2e84f0758af387d60c', [System.Globalization.NumberStyles]::AllowHexSpecifier)
Write-Host "Cipher Text"
Write-Host $c.ToString('x') -Fore Red
 
$d = [System.Numerics.BigInteger]::ModPow($c, $d, $r)
Write-Host $d.ToString('x') -Fore Magenta
Write-Host $d -Fore Magenta
$thing = $d.ToByteArray()
[Array]::Reverse($thing)
$thing2 = [System.Text.Encoding]::ASCII.GetString($thing)
$thing2
 
$test = [System.Numerics.BigInteger]::ModPow($d, $e, $r)
Write-Host $test.ToString('x') -Fore Magenta
 
 
$someString = "random_prime_gen"
$md5 = new-object -TypeName System.Security.Cryptography.MD5CryptoServiceProvider
$utf8 = new-object -TypeName System.Text.UTF8Encoding
$hash = [System.BitConverter]::ToString($md5.ComputeHash($utf8.GetBytes($someString)))
$hash
 
 
 
 
#Factored Here:https://www.alpertron.com.ar/ECM.HTM
 
#R = 15196548805163675574438244877329263428577430669415450686061847979005749203
#PHI = 5196548805163675541314925560689330810874068930793358063959256541686016000
 
 
#Message = 6394120318487837105297192000999263103028083158543266158077074961769623181
 
#8623129973970856246767140274286271280108982937367039574052688746406820632