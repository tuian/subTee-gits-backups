<#
 
    ElGamal in PowerShell
    by Casey Smith @subTee
     
 
The key generator works as follows:
Alice generates an efficient description of a cyclic group G of order q ,with generator g. 
Alice chooses an x randomly from 1 - (q-1)
Alice computes h = g^x.
Alice publishes h along with the description of G, q, g as her public key. Alice retains x as her private key, which must be kept secret.
 
Encryption:
The encryption algorithm works as follows: to encrypt a message m to Alice under her public key (G,q,g,h),
 
Bob chooses a random y from (1...q-1), then calculates c_1 = g^y.
Bob calculates the shared secret s = h^y.
Bob maps his secret message m onto an element m' of G. (Inverse Mod)
Bob calculates c_2 = m'(s) 
Bob sends the ciphertext (c_1,c_2) = (g^y, m'(h^y)) = (g^y, m'(g^x)^y) to Alice.
Note that one can easily find h^y if one knows m'. Therefore, a new y is generated for every message to improve security. For this reason, y is also called an ephemeral key.
 
Decryption:
The decryption algorithm works as follows: to decrypt a ciphertext (c_1,c_2) with her private key x,
 
Alice calculates the shared secret s = c_1^x
and then computes m' = c_2(s^(-1)) which she then converts back into the plaintext message m, where s^{-1} is the inverse of s in the group G. (E.g. modular multiplicative inverse if G is a subgroup of a multiplicative group of integers modulo n).
The decryption algorithm produces the intended message.
     
//Free Large Known Primes For Testing
//https://primes.utm.edu/lists/small/small.html
#>
 
[Reflection.Assembly]::LoadWithPartialName("System.Security")
 
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
 
$One = [System.Numerics.BigInteger] 1
$Two =  [System.Numerics.BigInteger] 2
$p = New-Object System.Numerics.BigInteger
$result = [System.Numerics.BigInteger]::TryParse("5371393606024775251256550436773565977406724269152942136415762782810562554131599074907426010737503501", [ref] $p)
$etotient = [System.Numerics.BigInteger]::Subtract($p, $One)
$g = [System.Numerics.BigInteger] 3
$e = [System.Numerics.BigInteger]::Divide($etotient,$Two)
#Find Generator
while([System.Numerics.BigInteger]::ModPow($g, $e, $p) -ne $etotient ){ $g = [System.Numerics.BigInteger]::Add($g, $Two) }
$n = 2048
$rngAlice = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
[byte[]] $bytesa = New-Object Byte[] ($n / 8)
$rngAlice.GetBytes($bytesa)
[System.Numerics.BigInteger] $x = (New-Object System.Numerics.BigInteger -ArgumentList @(,$bytesa)) % $p
if($x -lt [System.Numerics.BigInteger]::Zero) { $x = [System.Numerics.BigInteger]::Add($x, $p) } 
[System.Numerics.BigInteger] $h = [System.Numerics.BigInteger]::ModPow($g, $x, $p)
Write-Host $h, $p, $g -Fore Yellow
 
#Encrypt Message
$rngBob = New-Object System.Security.Cryptography.RNGCryptoServiceProvider
[byte[]] $bytesb = New-Object Byte[] ($n / 8)
$rngBob.GetBytes($bytesb)
[System.Numerics.BigInteger] $y = (New-Object System.Numerics.BigInteger -ArgumentList @(,$bytesb)) % $p
if($y -le [System.Numerics.BigInteger]::Zero ) {$y = [System.Numerics.BigInteger]::Add($y, $p) } 
#Compute Secret Message
[System.Numerics.BigInteger] $c_1 = [System.Numerics.BigInteger]::ModPow($g, $y, $p)
[System.Numerics.BigInteger] $s = [System.Numerics.BigInteger]::ModPow($h, $y, $p)
[System.Numerics.BigInteger] $message = [System.Numerics.BigInteger] 123
[System.Numerics.BigInteger] $minv = invmod $message $p
[System.Numerics.BigInteger] $c_2 = ([System.Numerics.BigInteger]::Multiply($minv, $s)) % $p
Write-Host $c_1 $c_2 -Fore Magenta
 
#Decrypt Message
[System.Numerics.BigInteger] $s1 = [System.Numerics.BigInteger]::ModPow($c_1, $x, $p)
[System.Numerics.BigInteger] $sinv = invmod $s1 $p
[System.Numerics.BigInteger] $minv1 = ([System.Numerics.BigInteger]::Multiply($sinv, $c_2)) % $p
[System.Numerics.BigInteger] $decrypt = invmod $minv1 $p
Write-Host $decrypt -Fore Green