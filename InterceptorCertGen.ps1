function Invoke-CreateCertificate([string] $certSubject, [bool] $isCA)
{
    $CAsubject = $certSubject
    $dn = new-object -com "X509Enrollment.CX500DistinguishedName"
    $dn.Encode( "CN=" + $CAsubject, $dn.X500NameFlags.X500NameFlags.XCN_CERT_NAME_STR_NONE)
    #Issuer Property for cleanup
    $issuer = "__Interceptor_Trusted_Root"
    $issuerdn = new-object -com "X509Enrollment.CX500DistinguishedName"
    $issuerdn.Encode("CN=" + $issuer, $dn.X500NameFlags.X500NameFlags.XCN_CERT_NAME_STR_NONE)
    # Create a new Private Key
    $key = new-object -com "X509Enrollment.CX509PrivateKey"
    $key.ProviderName =  "Microsoft Enhanced RSA and AES Cryptographic Provider" #"Microsoft Enhanced Cryptographic Provider v1.0"  
    # Set CAcert to 1 to be used for Signature
    if($isCA)
        {
            $key.KeySpec = 2 
        }
    else
        {
            $key.KeySpec = 1
        }
    $key.Length = 2048
    $key.MachineContext = 1
    $key.Create() 
      
    # Create Attributes
    $serverauthoid = new-object -com "X509Enrollment.CObjectId"
    $serverauthoid.InitializeFromValue("1.3.6.1.5.5.7.3.1")
    $ekuoids = new-object -com "X509Enrollment.CObjectIds.1"
    $ekuoids.add($serverauthoid)
    $ekuext = new-object -com "X509Enrollment.CX509ExtensionEnhancedKeyUsage"
    $ekuext.InitializeEncode($ekuoids)
 
    $cert = new-object -com "X509Enrollment.CX509CertificateRequestCertificate"
    $cert.InitializeFromPrivateKey(2, $key, "")
    $cert.Subject = $dn
    $cert.Issuer = $issuerdn
    $cert.NotBefore = (get-date).AddDays(-1) #Backup One day to Avoid Timing Issues
    $cert.NotAfter = $cert.NotBefore.AddDays(90) #Arbitrary... Change to persist longer...
    #Use Sha256
    $hashAlgorithmObject = New-Object -ComObject X509Enrollment.CObjectId
    $hashAlgorithmObject.InitializeFromAlgorithmName(1,0,0,"SHA256")
    $cert.HashAlgorithm = $hashAlgorithmObject
    #Good Reference Here http://www.css-security.com/blog/creating-a-self-signed-ssl-certificate-using-powershell/
     
    $cert.X509Extensions.Add($ekuext)
    if ($isCA)
    {
        $basicConst = new-object -com "X509Enrollment.CX509ExtensionBasicConstraints"
        $basicConst.InitializeEncode("true", 1)
        $cert.X509Extensions.Add($basicConst)
    }
    else
    {              
        $signer = (Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -match "__Interceptor_Trusted_Root" })
        $signerCertificate =  new-object -com "X509Enrollment.CSignerCertificate"
        $signerCertificate.Initialize(1,0,4, $signer.Thumbprint)
        $cert.SignerCertificate = $signerCertificate
    }
    $cert.Encode()
 
    $enrollment = new-object -com "X509Enrollment.CX509Enrollment"
    $enrollment.InitializeFromRequest($cert)
    $certdata = $enrollment.CreateRequest(0)
    $enrollment.InstallResponse(2, $certdata, 0, "")
 
    if($isCA)
    {              
                                     
        # Need a Better way to do this...
        $CACertificate = (Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -match "__Interceptor_Trusted_Root" })
        # Install CA Root Certificate
        $StoreScope = "LocalMachine"
        $StoreName = "Root"
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store $StoreName, $StoreScope
        $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $store.Add($CACertificate)
         
                                     
    }
    else
    {
        return (Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -match $CAsubject })
    } 
      
}
 
 
Invoke-CreateCertificate "__Interceptor_Trusted_Root" $true
Invoke-CreateCertificate "www.example.com" $true
(Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -match "__Interceptor_Trusted_Root"  })
(Get-ChildItem Cert:\LocalMachine\My | Where-Object {$_.Subject -match "www.example.com"  })
 
function Invoke-RemoveCertificates([string] $issuedBy)
{
    $certs = Get-ChildItem cert:\LocalMachine\My | where { $_.Issuer -match $issuedBy }
    if($certs)
    {
        foreach ($cert in $certs) 
        {
            $store = Get-Item $cert.PSParentPath
            $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::MaxAllowed)
            $store.Remove($cert)
             
        }
    }
    #Remove Any Trusted Root Certificates
    $certs = Get-ChildItem cert:\LocalMachine\Root | where { $_.Issuer -match $issuedBy }
    if($certs)
    {
    foreach ($cert in $certs) 
        {
            $store = Get-Item $cert.PSParentPath
            $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::MaxAllowed)
            $store.Remove($cert)
             
        }
    }
 
    #Remove Any Intermediate CA Certificates                                               #spaceB0x!
    $certs = Get-ChildItem cert:\LocalMachine\CA | where { $_.Issuer -match $issuedBy }
    if($certs)
    {
    foreach ($cert in $certs) 
        {
            $store = Get-Item $cert.PSParentPath
            $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::MaxAllowed)
            $store.Remove($cert)
             
        }
    }
 
    [Console]::WriteLine("Certificates Removed")
         
}
 
Invoke-RemoveCertificates( "__Interceptor_Trusted_Root" )
Invoke-RemoveCertificates( "www.example.com" )